namespace sand.Util {
    public enum Importance {
        High,
        Low,
    }
    public interface Error { 
        Importance Priority { get; set; }
        string Report();
    }
}