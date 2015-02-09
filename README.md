![](HivemindLogo.png?raw=true)

# Hivemind

Hivemind is a [Behavior Tree](http://en.wikipedia.org/wiki/Behavior_Trees_(Artificial_Intelligence,_Robotics_and_Control)) implementation for Unity3D that features a visual editor with runtime visual debugging capabilities.

![Screenshot](Screenshot.png?raw=true)

## Warning!

This project is in early development, and while everything described bellow is currently functional, keep in mind:

- Many node types are simply not yet implemented (a warning will be displayed in the inspector for these) and will always return the Error status.
- There might be many undiscovered bugs.
- Performance will probably degrade rapidly with very large trees or many concurrent agents.

## What is a Behavior Tree?

Behavior Trees are widely used in the game development industry (as well as other fields such as robotics) that allows a designer to map a complex set of branching actions, sensors and conditions that simulate inteligent behavior in an artificial agent, be it an individual game character or a virtual oponent or ally controlling multiple agents or game mechanics.

## How to use Hivemind to implement Behavior Trees in your game

### The Behavior Tree asset

A Hivemind `Behavior Tree` is a reusable custom assets that you store in your Unity project, and behave similarly to the native `Animator Controller`.  

As any other asset, simply chose `Assets / Create / Behavior Tree` to create a Hivemind Behavior Tree asset in your project.  

With a Behavior Tree asset selected in your Project view, you can bring up the visual editor by clicking `Window / Behavior Tree Editor`.  

A newly created Behavior Tree will have a single "root" node. Most of the visual editor functionality is controlled via right-click context menus on the nodes and the editor background, as well as the Unity3D inspector for configuring selected nodes.

### The Behavior Tree Agent component

`Behavior Tree Agent` is a component that can be added to any `GameObject`, that lets you assign an existing Behavior Tree to that Game Object to create AI enabled agents in your game.

### Action Libraries

Actions are mapped to methods in classes that inherit from `Hivemind.ActionLibrary`. The action library methods can then be decorated with the `[Hivemind.Action]` attribute, making that action available in the visual editor.  

##### Variables

Action Libraries have two inherited variables:

- `agent`: holds a reference to the current agent
- `context`: a dictionary-like structure that can be used to pass data between actions, instantiated once per behavior tree, per agent

##### Implementing Action Libraries

Actions can be as generic as the designer wants them, so that they can be reused as much as possible in the project.  

An action library class is instantiated (on demand) once per agent, per behavior tree. The behavior tree will call the `Start` function of the action library as soon as the library is instantiated and the agent and context variables have been populated, so the user can do any necessary initialization.  

Multiple Action Libraries can be used in a Behavior Tree, but each will only be instantiated once.  

Actions __must__ return a value from the `Hivemind.Status` enum, which include:

- `Success`
- `Failure`
- `Running`
- `Error`

##### Parameters

The method parameters can be used to configure an action node. Currently, only the C# primitives `string` and `float` are allowed. More C# primitives and other value types, as well as common Unity3D object references are planned for future implementation.

##### Context

The current Context, accessible via the `context` variable inherited from `Hivemind.ActionLibrary`, is a key-value store similar to dictionaries, but implemented as a C# generic. It has the following public methods:

- `Set<T>(string key, T value)`: Sets a value of type T
- `Get<T>(string key)`: Retrieves a value of type T
- `Unset(string key)`: Removes a value from the context

Action methods can be decorated with attributes that hint to Context values that method expects or populates, and these will show up on the visual editor inspector:

- `[Hivemind.Expects(string key, Type type)]`
- `[Hivemind.Outputs(string key, Tyoe type)]`

In the future, a design-time debugging feature will ensure context values required by an action are populated in previous actions. For the moment, a designer can use the above attributes as reference.

### Debugging

To watch the execution of a behavior tree in real time, make sure you have selected a _Game Object_ that has a _Behavior Tree Agent_ component attached, and open the _Behavior Tree Editor window_. The screenshot above illustrates a runing Behavior Tree.  

Additionaly, the current _Context_ is displayed in the _Behavior Tree Agent_ component inspector.