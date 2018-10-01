using System;

namespace Sodev.Marten.Base.Model
{
    [Serializable]
    public class PidParameters
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public int ReturnedBytesNb { get; set; }

        public int MinValue { get; set; }

        public int MaxValue { get; set; }

        public string Unit { get; set; }

        public string Formula { get; set; }
    }
}
