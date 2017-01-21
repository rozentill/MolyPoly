using System;

public static class MaybeExtension {
	public static Maybe<O> Select<I, O>(this Maybe<I> m, Func<I, O> f) {
		if (m.any)
						return new Just<O> (f (m.value));
				else
						return new None<O> ();
	}
	public static Maybe<O> Bind<I, O>(this Maybe<I> m, Func<I, Maybe<O>> f) {
		if (m.any)
			return f (m.value);
		else
			return new None<O> ();
	}

	public static Maybe<I> Reduce<I>(this Maybe<Maybe<I>> m) {
		return m.otherwise (new None<I> ());
	}

	public static Maybe<I> NotNull<I>(this I v) where I : class {
		if (v == null)
						return new None<I> ();
				else
						return new Just<I> (v);
	}
}
public abstract class Maybe<T> {

	protected Maybe(bool any) { this.any = any; }
	public readonly bool any;

	public abstract T value { get; }

	public T otherwise(T d) {
		if (any)
						return value;
				else
						return d;
	}
}

public class Just<T> : Maybe<T> {
	public Just(T value) : base(true) {
		_value = value;
	}
	
	public override T value {
		get {
			return _value;
		}
	}
	
	private readonly T _value;
}

public class None<T> : Maybe<T> {
	public None() : base(false) {}
	
	public override T value {
		get {
			throw new System.InvalidOperationException();
		}
	}
}