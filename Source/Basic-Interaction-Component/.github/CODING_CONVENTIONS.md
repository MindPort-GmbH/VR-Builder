# Coding Conventions

Unless otherwise specified, the Innoactive projects adhere to the MSDN C# [Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions) and [Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/).
In the following, the most important guidelines and custom decisions are listed.

## Language
* Code, documentation, and comments are to be written in English.

## Naming Conventions

### Name
* Use meaningful but concise names.
* Do not use special characters in any names.
* Do not use abbreviations.

```cs
public class RuntimeStatistics
{
    // very bad:
    private int n;
    // bad:
    private int numFrms;
    private int frameCnt;
    // good:
    private int numberOfFrames;
    private int frameCount;
    // too verbose:
    private int numberOfElapsedFramesSinceApplicationStart;
}
```

* Keep the most specific descriptor left, e.g. pauseButton not buttonPause.
* Boolean variables and Properties should start with Is or Has, e.g. isActive.

### Formatting
* Classes, properties, and public fields are written in PascalCase.
* Private fields are written in camelCase.
* Static and const fields (public, protected, or private) are formatted like non-const/static fields.

```cs
public class MyClass
{
    public float MyProperty { get; private set; }
    public bool MyPublicVariable;
    private string myPrivateVariable;
    public static string MyStaticVariable = "someValue";
    protected const string MyConstVariable = "someOtherValue";
    private const string myPrivateConstVariable = "yetAnotherValue";
}
```

* Functions are written in PascalCase.
* Function parameters are written in camelCase.
* Local variables are written in camelCase.

```cs
public bool MyFunction(bool myFunctionParameter)
{
    float myLocalVariable = 0;
}
```

* Interfaces are written in PascalCase, but start with an *I*.

```cs
public interface IMyInterface
{
}
```

* Enums are written in PascalCase.
* Enum Values are written in PascalCase.

```cs
public enum MyEnum
{
    FirstEnumValue,
    SecondEnumValue,
    ThirdEnumValue,
}
```

## Bracing and Indentation
* Each opening and closing brace is placed in a separate line.
* Braces start at the indentation level of the outlying scope.
* Add Braces even for short single-line scopes.

```cs
// Do
if (condition == true)
{
    return;
}
// Don't
if (condition == true)
    return;
if (condition == true) return;
```

* Exception: Default properties are implemented in a single line.
  Encapsulating properties and properties with simple single-statement accessors place accessors on a single line together with the respective implementation.
  Complex accessors with multiple statements follow the general rule of placing braces and statements on separate lines.

```cs
public bool DefaultProperty { get; protected set; }

public bool EncapsulatingProperty
{
    get { return wrappedValue; }
    protected set { wrappedValue = value; }
}

public bool SimpleProperty
{
    get { return entry == "MagicValue"; }
    set { entry = string.Format("Bla{0}", value); }
}

public bool ComplexProperty
{
    get
    {
        if(wrappedValue == null)
        {
            throw new OperationException();
        }

        wrappedValue;
    }
    protected set
    {
        wrappedValue = value;
        
        if (implementation != null)
        {
            implementation.ForwardValue(value);
        }
    }
}

public bool MixedProperty
{
    get
    {
        return wrappedValue;
    }
    protected set
    {
        wrappedValue = value;
        
        if (implementation != null)
        {
            implementation.ForwardValue(value);
        }
    }
}
```

* Content of the braced scope is indented an additional level.
* Indent by 4 spaces.
* Indent every scope (namespaces, classes, loops, ...).

```cs
public class ClassInTopScope
{
    public bool PropertyInClassScope { get; set; }
    
    public float FunctionInClassScope()
    {
        public float functionVariable;

        {
            float scopedVariable = 3;
        }

        for (int i = 0; i < 100; ++i)
        {
            float scopedVariable = i;
        }
    }
}
```

* Do not indent compile `#if` and `#pragma`.
* Indent `#region` specifications.
* Do not further indent code inside compile directives like `#if` or `#region`s.

```cs
namespace MyNamespace
{
    public class MyClass1
    {
        public bool FirstFunction();

#pragma warning disable 0618 // temporarily disable obsolete usage warning
        CallObsoleteFunction();
#pragma warning restore 0618

        #region First Region
        public bool SecondFunction();
        #endregion

        #region Second Region
        public bool ThirdFunction();
        #endregion
    }

#if COMPILE_DEFINE
    public class MyClass2
    {
    }
#endif
}
```

* When splitting a statement or declaration, indent the split line by one level.
* When defining local delegates, do not indent the opening and closing braces.

```cs
public float FunctionWithManyParameters()
{
    bool expression = CheckConditionOne() 
        && CheckConditionTwo()
        && CheckConditionThree() 
        && CheckConditionFour();

    Action localTask = () =>
    {
        // [...]
    }
    
    new AsyncTask(localTask).Execute();

    new AsyncTask(() => 
    {
        // [...]
    })
    .Execute();
}
```

## Code comments
* Enrich your code with meaningful comments.
* Use `//` for inline code comments, and `///` for API docs.
* Add xml API docs to all elements of the public API: classes, functions, properties, fields, and events.
* Comments should always start with a capital letter, be separated from the comment symbol by a space, and end with a period.
* All sentence-like parts of a comment (e.g. `<param> and <return>` texts) should end with a period.


```cs
/// <summary>
/// Explain the general intent and typical usage of the class here.
/// </summary>
public class ExampleClass
{
    /// <summary>
    /// Provide additional detail here.
    /// </summary>
    public float ExampleProperty { get; private set; }

    /// <summary>
    /// Provide details about what the function does.
    /// </summary>
    /// <param name="exampleParameter">Explain parameter here.</param>
    /// <returns>Explain return type here.</returns>
    /// <exception cref="ExampleException">When is this exception thrown.</exception>
    public int ExampleFunction(float exampleParameter)
    {
        // [...]
    }
}
```

* Avoid empty xml tags components like `param` or `return`. Either add a valuable descriptions, or remove the tag.

```cs
public class ExampleClass
{
    //Do

    /// <summary>
    /// This function computes the square root of a value.
    /// </summary>
    public float SquareRoot(float value)
    {
        // [...]
    }

    // Don't

    /// <summary>
    /// This function computes the square root of a value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public float SquareRoot(float value)
    {
        // [...]
    }
}
```

## Whitespaces
* Use empty lines to separate classes/class members/blocks of statements.
* Do not use two or more consecutive empty lines (use regions instead to group blocks).
* Add a whitespace to separate a condition/iteration keyword (`if`, `while`, `for`, ...) and its braces.
* Do not use a whitespace to separate a function name and its brace.

```cs
if (CheckParameters(param1, param2) && param3)
{
    // [...]
}

while (pi == 3)
{
    // [...]
}

SomeFunction(param);
```

* Add whitespaces after commas and around operators.

```cs
if (CheckParameters(param1, param2) && param3)
{
    return param1 + (param2 * param3);
}
```

## Events

* Use C# events - avoid UnityEvents, Actions, or similar.
* Use EventArgs classes instead of delegates or actions for event definitions.
* Use immutable `EventArgs` classes, which provide public readonly fields. Initialize these fields from a constructor.

```cs
public class MyButton
{
    // Do
    public class ClickedEventArgs : System.EventArgs
    {
        public readonly string SomeData;
        public ClickedEventArgs(string data)
        {
            SomeData = data;
        }
    }

    public event EventHandler<ClickedEventArgs> Clicked;

    // Don't
    public delegate void ClickedEvent(string someData);
    public event ClickedEvent Clicked;

    // Don't
    public event Action<string> Clicked;
}
```

* EventArgs classes end with `EventArgs`, and are derived from `System.EventArgs`.
* Name Events with a verb describing the event (without pre- or suffix), Loaded, Finished.
* Event handling functions start with `On`, followed by the scope and the event name.
* If an event can be triggered by a child class, provide a function called `EmitEVENTNAME`.

```cs
public class MyButton
{
    public class ClickedEventArgs : System.EventArgs
    {
        public readonly string someData;
        public ClickedEventArgs(string data)
        {
            someData = data;
        }
    }

    public event EventHandler<ClickedEventArgs> Clicked;

    protected void EmitClicked(string data)
    {
        Clicked.Invoke(this, new ClickedEventArgs() { someData = data });
    }
}

public class ButtonObserver
{
    public void Observe
    {
        new MyButton().Clicked += OnButtonClicked;
    }

    private void OnButtonClicked(object sender, MyButton.ClickedEventArgs args)
}
```

## Logging

* Prefer format syntax over explicit string concatenation.

```cs
private void SomeFunction()
{
    int numberOfThings = 42;

    // Do
    Debug.Log($"I know {numberOfThings} things");

    // Or
    Debug.LogFormat("I know {0} things", numberOfThings);

    // Don't
    Debug.Log("I know " + numberOfThings + " things");
}
```

## Exceptions

* Exception names end with `Exception`.
* If exceptions are to be thrown by only one class, define them inside this class.
* Exceptions thrown by multiple sources are defined in a separate file, in the matching namespace (no Exceptions sub-namespace).

## Access Modifiers

* Only use public variables or Properties with public setters for data objects.
* Always specify an Access Modifier.

```cs
public class MyClass
{
    // Do
    private float myPrivateVariable1;
    // Don't
    float myPrivateVariable2;
}
```

## Unity-specific

* Prefer private variables marked with `[SerializeField]` over public variables for Inspector fields.
* When using a public variable in a MonoBehaviour, still add `[SerializeField]` to ease detection of Inspector fields.
* Use the `[Tooltip("...")}` attribute in Inspector fields for documentation / usage hints.
* Always initialize Inspector fields (to prevent never-assigned-warnings).
* Avoid the automatic conversion from `UnityEngine.Object` to bool.
* When using Enum-typed Inspector fields, consider hard-wiring the enum values to prevent serialization errors when changing available enum values.

```cs
// Do
if (gameObject != null)
{
    // [...]
}

// Don't
if (gameObject)
{
    // [...]
}
```

## Lambdas

* Prefer lambda-style expressions over delegate-style.

```cs
// Do
(float param1, string param2) => {}

// Don't
delegate(float param1, string param2) {}
```

* Prefer named functions over lambdas.
* Assign longer or more complex lambdas to named local `Action`/`Func`.
* If nested lambdas are required, assign each one to named local `Action`/`Func`.
* Avoid defining long lambdas directly as parameter of a function call.

## Obsoletion

* Avoid changes in code that break existing APIs (i.e. Class Names, Function Argument types, Renaming of public members / properties, return types).
* Use `System.Obsolete` to mark outdated code that will be removed in newer versions.
* When re-naming classes/properties/methods, try to maintain the old version tagged as obsolete.
* Obsolete code will be maintained for one (major) release, and is deleted afterwards.

## Misc

* Define class-specific Exceptions, Enums, EventArgs, ... inside the corresponding class.
* For Interfaces, define Exceptions, Enums, EventArgs, ... in the same file as the interface.
* Do not use `var`, always specify the full type name.
* Use `Invoke` to trigger actions or events.
* Only use `this.` before members where absolutely necessary.
* Decide whether or not to design for inheritance.
  * If not intended to be derived from, make all non-public entities `private`.
  * If intended to be derived from, take care to mark necessary entities `protected` and/or `virtual`.
* Consider using early returns in functions to prevent deeply nested scopes.
* Use parentheses to group parts of complex statements.
  Especially, always group sub-clauses when combining different boolean operators.
* Do not omit the array type specifier when using array initialization lists.

```cs
// Do
string[] myList = new string[] { "a", "b", "c", "d" };

// Don't
string[] myList = new [] { "a", "b", "c", "d" };
```

* Do not prefix static method defined in the same or parent class with the class scope.