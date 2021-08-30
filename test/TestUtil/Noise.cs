namespace test.TestUtil {
    public record Noise(ulong seed) {

        public ulong Any() {
            var x = seed + 17;
            
            x ^= x << 13;
            x ^= x >> 7;
            x ^= x << 17;

            return x;
        }

        public Noise Next() => this with {seed = this.Any()};

        public ulong InRange(ulong min, ulong max) {
            var m = (max - min) + 1;
            return seed % m + min;
        }

        public ulong Max(ulong max) => InRange(0, max);
    }
}