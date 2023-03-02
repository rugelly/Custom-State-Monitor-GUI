# Custom-State-Monitor-GUI
## also includes the necessary State Machine made from ScriptableObjects
### GUI:
- Searches for all instances of State
- applies filter flags for issues that need to be addressed
- shows selected States actions + transitions + transitions' linked state
### ScriptableObject State Machine
- States: hold actions + connections
- -Action: self contained, runs every time the current state is ticked
- -Connection: holds a State, if evals true the current state will change to held state
- --connections are eval'd that if you enter a state and can leave it immediately you will before anything else is called
