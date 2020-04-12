using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SimpleTasks
{
    internal abstract class ImmutableStack<T> : IEnumerable<T>, IEnumerable
    {
        public abstract ImmutableStack<T> Push(T value);
        public abstract ImmutableStack<T> Pop();
        public abstract ImmutableStack<T> PopOrEmpty();
        public abstract T Peek();
        [return: MaybeNull]
        public abstract T PeekOrDefault();
        public abstract bool IsEmpty { get; }
        public abstract int Count { get; }

        private sealed class EmptyStack : ImmutableStack<T>
        {
            public override bool IsEmpty => true;
            public override int Count => 0;
            public override T Peek() => throw new Exception("Empty stack");
            [return: MaybeNull]
            public override T PeekOrDefault() => default;

            public override ImmutableStack<T> Push(T value) => new ImmutableStack<T>.NonEmptyStack(value, this);
            public override ImmutableStack<T> Pop() => throw new Exception("Empty stack");
            public override ImmutableStack<T> PopOrEmpty() => Empty;
        }

        private sealed class NonEmptyStack : ImmutableStack<T>
        {
            private readonly T head;
            private readonly ImmutableStack<T> tail;

            public override bool IsEmpty => false;
            public override int Count { get; }

            public NonEmptyStack(T head, ImmutableStack<T> tail)
            {
                this.head = head;
                this.tail = tail;
                this.Count = tail.Count + 1;
            }

            public override T Peek() => this.head;
            public override T PeekOrDefault() => this.head;
            public override ImmutableStack<T> Pop() => this.tail;
            public override ImmutableStack<T> PopOrEmpty() => this.tail;
            public override ImmutableStack<T> Push(T value) => new ImmutableStack<T>.NonEmptyStack(value, this);
        }

        public static ImmutableStack<T> Empty { get; } = new EmptyStack();

        public IEnumerator<T> GetEnumerator()
        {
            for (ImmutableStack<T> stack = this; !stack.IsEmpty; stack = stack.Pop())
                yield return stack.Peek();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    internal static class ImmutableStack
    {
        public static ImmutableStack<T> Init<T>(T initialValue) => ImmutableStack<T>.Empty.Push(initialValue);
        public static ImmutableStack<T> From<T>(IEnumerable<T> initialValues)
        {
            var stack = ImmutableStack<T>.Empty;
            foreach (var initialValue in initialValues)
            {
                stack = stack.Push(initialValue);
            }
            return stack;
        }
    }
}
