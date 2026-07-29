using Mono.Cecil;
using Mono.Cecil.Cil;

namespace C2CS.Extractor.DotNet;

/// <summary>Abstract values tracked by the simulator.</summary>
public abstract record Value
{
    public sealed record Str(string V, bool Derived) : Value;

    public sealed record Int(int V) : Value;

    /// <summary>Came from a method parameter — the wrapper signal.</summary>
    public sealed record Param : Value;

    public sealed record Unknown : Value;
}

/// <summary>
/// Linear, intraprocedural IL value tracker. Simulates one pass over a method body,
/// tracking constant strings/ints through the evaluation stack, locals, and
/// string.Concat of known values. Deliberately conservative: any branch clears the
/// stack (statement boundaries have empty stacks in compiled C#; locals survive), and
/// anything unhandled becomes Unknown. Never guesses (the extractor's prime rule).
/// </summary>
public sealed class StackSimulator
{
    private readonly List<Value> _stack = new();
    private readonly Dictionary<int, Value> _locals = new();

    /// <summary>
    /// Runs through the body; invokes <paramref name="atCall"/> with the current stack
    /// (top = last element) just before each call/callvirt/newobj executes.
    /// </summary>
    public void Run(MethodDefinition method, Action<Instruction, IReadOnlyList<Value>> atCall)
    {
        _stack.Clear();
        _locals.Clear();

        foreach (var ins in method.Body.Instructions)
        {
            var code = ins.OpCode.Code;
            if (code is Code.Call or Code.Callvirt or Code.Newobj)
            {
                atCall(ins, _stack);
                ExecuteCall(ins);
                continue;
            }

            Execute(ins);
        }
    }

    private void Execute(Instruction ins)
    {
        switch (ins.OpCode.Code)
        {
            case Code.Nop:
            case Code.Break:
                return;
            case Code.Ldstr:
                Push(new Value.Str((string)ins.Operand, Derived: false));
                return;
            case Code.Ldc_I4: Push(new Value.Int((int)ins.Operand)); return;
            case Code.Ldc_I4_S: Push(new Value.Int((sbyte)ins.Operand)); return;
            case Code.Ldc_I4_0: Push(new Value.Int(0)); return;
            case Code.Ldc_I4_1: Push(new Value.Int(1)); return;
            case Code.Ldc_I4_2: Push(new Value.Int(2)); return;
            case Code.Ldc_I4_3: Push(new Value.Int(3)); return;
            case Code.Ldc_I4_4: Push(new Value.Int(4)); return;
            case Code.Ldc_I4_5: Push(new Value.Int(5)); return;
            case Code.Ldc_I4_6: Push(new Value.Int(6)); return;
            case Code.Ldc_I4_7: Push(new Value.Int(7)); return;
            case Code.Ldc_I4_8: Push(new Value.Int(8)); return;
            case Code.Ldc_I4_M1: Push(new Value.Int(-1)); return;
            case Code.Dup:
                Push(_stack.Count > 0 ? _stack[^1] : new Value.Unknown());
                return;
            case Code.Pop:
                PopN(1);
                return;
            case Code.Stloc_0: StoreLocal(0); return;
            case Code.Stloc_1: StoreLocal(1); return;
            case Code.Stloc_2: StoreLocal(2); return;
            case Code.Stloc_3: StoreLocal(3); return;
            case Code.Stloc:
            case Code.Stloc_S:
                StoreLocal(((VariableDefinition)ins.Operand).Index);
                return;
            case Code.Ldloc_0: LoadLocal(0); return;
            case Code.Ldloc_1: LoadLocal(1); return;
            case Code.Ldloc_2: LoadLocal(2); return;
            case Code.Ldloc_3: LoadLocal(3); return;
            case Code.Ldloc:
            case Code.Ldloc_S:
                LoadLocal(((VariableDefinition)ins.Operand).Index);
                return;
            case Code.Ldarg_0:
            case Code.Ldarg_1:
            case Code.Ldarg_2:
            case Code.Ldarg_3:
            case Code.Ldarg:
            case Code.Ldarg_S:
                Push(new Value.Param());
                return;
            case Code.Ret:
            case Code.Throw:
            case Code.Leave:
            case Code.Leave_S:
            case Code.Endfinally:
                _stack.Clear();
                return;
        }

        if (ins.OpCode.FlowControl is FlowControl.Branch or FlowControl.Cond_Branch)
        {
            // Statement boundaries have empty stacks in compiled C#; a value that
            // crosses a branch is beyond linear tracking. Locals survive.
            _stack.Clear();
            return;
        }

        // Generic fallback: honest arity bookkeeping, values become Unknown.
        PopN(PopCount(ins.OpCode.StackBehaviourPop));
        for (var i = 0; i < PushCount(ins.OpCode.StackBehaviourPush); i++)
            Push(new Value.Unknown());
    }

    private void ExecuteCall(Instruction ins)
    {
        var target = (MethodReference)ins.Operand;
        var pops = target.Parameters.Count + (target.HasThis && ins.OpCode.Code != Code.Newobj ? 1 : 0);

        // string.Concat over known strings stays known (Derived) — the one derivation
        // the PoC performs.
        if (target.DeclaringType.FullName == "System.String" && target.Name == "Concat"
            && target.Parameters.Count is 2 or 3 or 4
            && target.Parameters.All(p => p.ParameterType.FullName == "System.String")
            && TopN(target.Parameters.Count).All(v => v is Value.Str))
        {
            var parts = TopN(target.Parameters.Count).Cast<Value.Str>().Select(s => s.V);
            var concat = string.Concat(parts);
            PopN(pops);
            Push(new Value.Str(concat, Derived: true));
            return;
        }

        PopN(pops);
        var returnsValue = target.ReturnType.FullName != "System.Void" || ins.OpCode.Code == Code.Newobj;
        if (returnsValue) Push(new Value.Unknown());
    }

    /// <summary>Argument i (0-based, excluding instance) for a call about to execute.</summary>
    public static Value Argument(IReadOnlyList<Value> stack, MethodReference target, int i)
    {
        var fromTop = target.Parameters.Count - 1 - i;
        var index = stack.Count - 1 - fromTop;
        return index >= 0 && index < stack.Count ? stack[index] : new Value.Unknown();
    }

    private void Push(Value v) => _stack.Add(v);

    private void PopN(int n)
    {
        for (var i = 0; i < n && _stack.Count > 0; i++) _stack.RemoveAt(_stack.Count - 1);
    }

    private IEnumerable<Value> TopN(int n) =>
        _stack.Count >= n ? _stack.Skip(_stack.Count - n) : Enumerable.Repeat<Value>(new Value.Unknown(), n);

    private void StoreLocal(int index)
    {
        _locals[index] = _stack.Count > 0 ? _stack[^1] : new Value.Unknown();
        PopN(1);
    }

    private void LoadLocal(int index) =>
        Push(_locals.TryGetValue(index, out var v) ? v : new Value.Unknown());

    private static int PopCount(StackBehaviour b) => b switch
    {
        StackBehaviour.Pop0 => 0,
        StackBehaviour.Pop1 or StackBehaviour.Popi or StackBehaviour.Popref => 1,
        StackBehaviour.Pop1_pop1 or StackBehaviour.Popi_pop1 or StackBehaviour.Popi_popi
            or StackBehaviour.Popi_popi8 or StackBehaviour.Popi_popr4 or StackBehaviour.Popi_popr8
            or StackBehaviour.Popref_pop1 or StackBehaviour.Popref_popi => 2,
        StackBehaviour.Popi_popi_popi or StackBehaviour.Popref_popi_popi or StackBehaviour.Popref_popi_popi8
            or StackBehaviour.Popref_popi_popr4 or StackBehaviour.Popref_popi_popr8
            or StackBehaviour.Popref_popi_popref => 3,
        StackBehaviour.PopAll => int.MaxValue,
        _ => 0,
    };

    private static int PushCount(StackBehaviour b) => b switch
    {
        StackBehaviour.Push0 => 0,
        StackBehaviour.Push1 or StackBehaviour.Pushi or StackBehaviour.Pushi8
            or StackBehaviour.Pushr4 or StackBehaviour.Pushr8 or StackBehaviour.Pushref => 1,
        StackBehaviour.Push1_push1 => 2,
        _ => 0,
    };
}
