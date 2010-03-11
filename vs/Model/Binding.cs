namespace ZeroInstall.Model
{
    /// <summary>
    /// Bindings specify how the chosen implementation is made known to the running program.
    /// </summary>
    /// <remarks>
    /// Bindings can appear in <see cref="Dependency"/>s, in which case they tell a component how to find its dependency,
    /// or in <see cref="ImplementationBase"/>, where they tell a component how to find itself.
    /// </remarks>
    public abstract class Binding
    {}
}
