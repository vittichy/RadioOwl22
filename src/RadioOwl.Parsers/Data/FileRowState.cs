namespace RadioOwl.Parsers.Data
{
    /// <summary>
    /// stav radku s downloadem
    /// </summary>
    public enum FileRowState
    {
        None,

        /// <summary>
        /// Parsování web stránky pořadu
        /// </summary>
        Parse,

        /// <summary>
        /// Start stahování
        /// </summary>
        Started,

        Finnished,
        Error,
        AlreadyExists
    }
}
