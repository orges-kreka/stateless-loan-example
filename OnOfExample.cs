using Stateless;

namespace StatelessTest;

// This example has a simple state machine with only two states.
// The state information is of type string, and the type of the trigger is char.
public class OnOfExample
{
    const string on = "On";
    const string off = "Off";
    const char space = ' ';

    private readonly StateMachine<string, char> _onOffSwitch;

    public OnOfExample()
    {
        // initiate a new state machine in the 'off' state
         _onOffSwitch = new StateMachine<string, char>(off);
        
        // configure state machine with the Configure method,
        // supplying the state to be configured as a parameter.
        _onOffSwitch.Configure(off).Permit(space, on);
        _onOffSwitch.Configure(on).Permit(space, off);
    }

    public void Start()
    {
        Console.WriteLine("Press <space> to toggle the switch. Another key will exit the program.");
        
        while(true)
        {
            Console.WriteLine("Switch is in state: " + _onOffSwitch.State);
            var pressed = Console.ReadKey(true).KeyChar;
            
            // check if the user wants to exit
            if(pressed != space)break;
            
            // Use the fire method with the trigger as payload to supply the state machine with an event.
            // The state machine will react according to its configuration.
            
            _onOffSwitch.Fire(pressed);
        }
    }
}