using Assessment2.Model.Enums;

namespace Assessment2.Model
{
    internal class Boiler
    {
        public BoilerStatus Status { get; set; }
        public SwitchState Interlock { get; set; }

        public Boiler()
        {
            Status = BoilerStatus.Lockout;
            Interlock = SwitchState.Open;
        }
    }
}
