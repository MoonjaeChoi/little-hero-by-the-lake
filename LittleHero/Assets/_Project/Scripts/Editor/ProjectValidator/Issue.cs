namespace LittleHero.EditorTools
{
    public enum Severity { Error, Warning }

    public struct Issue
    {
        public Severity Sev;
        public string Category;
        public string Msg;

        public Issue(Severity s, string c, string m)
        {
            Sev = s;
            Category = c;
            Msg = m;
        }
    }
}
