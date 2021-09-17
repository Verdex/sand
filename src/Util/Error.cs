namespace sand.Util {
    public enum Importance {
        High,
        Low,
    }
    public interface Error { 
        Importance Priority();
        string Report();
    }
}