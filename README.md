# .NET Log Formatting
An extension of `Microsoft.Extensions.Logging` to format log information into typed entries.

### Features
* Select and transform log data into entries using a fluent API.
* Implement logger providers handling formatted entries.
* Convert batches of entries to binary data using JSON or a custom serializer.
* Direct serialized output to file system or a custom destination.

## Installation

Add the NuGet package to your project:

    $ dotnet add package Logging.Formatting

## Usage

Though .NET supports implementing custom logger providers, it's fairly limited in what tools it provides to work with log information.

In particular, it's often desireable to define the _format_ of log entries independently of their target/destination. Using generics, log formatting enables you to represent individual log entries using any mutable reference type - even one contained in a third party library.
