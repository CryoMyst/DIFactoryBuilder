# DIFactoryBuilder

A library that will generate Microsoft.DependencyInjection compatible factories for classes that need extra parameters. 

# Installation

[![latest version](https://img.shields.io/nuget/v/DIFactoryBuilder.SourceGenerator)](https://www.nuget.org/packages/DIFactoryBuilder.SourceGenerator)

IMPORTANT: Set output type as analyzer `OutputItemType="Analyzer"`
# Useage

Simply attribute your class with [RequiresFactory] and any parameter you wish to inject with [Inject]

## Example:
```cs
using System;
using System.Runtime;
using System.Collections.Generic;
using System.Collections;
using DIFactoryBuilder.Attributes;

namespace MyCode.TestNamespace
{
    [RequiresFactory]
    public class TestViewModel
    {
        public TestViewModel(
            string regularParameter, // Regular parameters get passed into the Create method parameters
            ICollection<IList<Int16>> genericParmameter, // Handles generic types
            [Inject] IEnumerable<IService> injectableParameter, // [Inject] parameters will use ServiceProvider.GetService<T>()
            int paramWithDefault = 3) // Defaults are handled
        {

        }
    }
}
```

Will produce the following factory class

```cs
using System;
using Microsoft.Extensions.DependencyInjection;

namespace MyCode.TestNamespace
{
    public class TestViewModelFactory : DIFactoryBuilder.IDIFactory<TestViewModel>
    {
        private readonly System.IServiceProvider _serviceProvider;
        public TestViewModelFactory(System.IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public TestViewModel Create(string regularParameter, 
            System.Collections.Generic.ICollection<System.Collections.Generic.IList<short>> genericParmameter, 
            int paramWithDefault = 3)
        {
            return new TestViewModel(regularParameter, 
                genericParmameter, 
                this._serviceProvider.GetService<System.Collections.Generic.IEnumerable<MyCode.TestNamespace.IService>>(), 
                paramWithDefault);
        }
    }
}
```

Which when registered will allow creation of the TestViewModel via the registered Service.

```cs
// Notice how the IService is ommitted as it is injected and default values are respected
var newViewModel = serviceProvider.GetService<TestViewModelFactory>().Create("SomeText", new List<IList<short>>());
```

## Automatically adding factorys
For ease of use an extension method exists which will register all Factories in the assembly
```cs
IServiceCollection.RegisterFactories(Assembly assembly)
```

## Contributing
Pull requests are welcome, ensure test converage.

# License
[GNU GPLv3](https://choosealicense.com/licenses/gpl-3.0/)