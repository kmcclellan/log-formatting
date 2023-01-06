namespace Microsoft.Extensions.Logging.Formatting;

/// <summary>
/// Formats stored log information into typed entries.
/// </summary>
/// <typeparam name="TFormat">The formatted entry type.</typeparam>
public interface ILogStackFormatter<in TFormat>
{
    /// <summary>
    /// Attempts to add state to the stack.
    /// </summary>
    /// <typeparam name="TState">The log state type.</typeparam>
    /// <param name="category">The log category name.</param>
    /// <param name="state">The log state.</param>
    /// <returns><see langword="true"/> if the state was added, otherwise <see langword="false"/>.</returns>
    bool TryPush<TState>(string category, TState state);

    /// <summary>
    /// Removes the state last added by <see cref="TryPush{TState}"/>.
    /// </summary>
    void Pop();

    /// <summary>
    /// Formats a typed entry using the current state.
    /// </summary>
    /// <param name="entry">The formatted entry.</param>
    void Format(TFormat entry);
}
