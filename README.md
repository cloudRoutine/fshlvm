## FsHlvm A High Level Virtual Machine Written In F#

FsHlvm is a cross-platform open-source virtual machine with the following features:

High-level DSL like language based on F#  
Safe  
Generics and type specialization  
Garbage collected  
High performance  
Multicore support  
Builtin FFI for C interoperability  
Commerce friendly  

The virtual machine is written in F# and uses the LLVM library for high-performance  
code generation.  

More information about FsHlvm are be available at:
http://www.kp-tech.hu/en/products/fshlvm-opensource

This work is based HLVM, written by Jon Harrop, Flying Frog Consultancy Ltd.  
More information about HLVM are available at: http://www.ffconsultancy.com/ocaml/hlvm/  

## Status

The `master` branch is for the latest version of FsHlvm.

## Build Requirements on Linux

Requires mono 3.4 or higher.  
Requires fsharp 3.1 or higher.  
Requires llvm-fs (packaged).

## Build Requirements on Windows

Requires .NET 4.5 or higher.  
Requires fsharp 3.1 or higher.  
Requires llvm-fs (packaged).  

## Execution Requirements on Linux

Tested on Ubuntu 14.04 (amd64)  

Requires LLVM-3.4  
Requires CLANG-3.4  
Requires FsHlvm fshlvmllvmwrapper shared library  
Requires FsHlvm fshlvmruntime shared library  

### Installing LLVM-3.4

apt-get install llvm-3.4-dev libllvm3.4

### Installing CLANG-3.4

apt-get install clang-3.4

## Execution Requirements on Windows (32bit)

Works in progress, only the LLVM codegen are working.

Please make sure you build the project using Release|x86 configuration, otherwise the .NET engine will throw BadImageFormatException.

https://github.com/CRogers/LLVM-Windows-Binaries/releases/download/v3.4/llvm-3.4-shared-library-windows.7z  
https://github.com/CRogers/LLVM-Windows-Binaries/releases/download/v3.4/llvm-3.4-tools-windows.7z  

Extract the llvm-3.4-shared-library-windows.7z and llvm-3.4-tools-windows.7z to a directory called %LLVM_PATH%.

You may also need to download and install the Visual C++ Redistributable Packages for Visual Studio 2013:  
http://www.microsoft.com/en-gb/download/details.aspx?id=40784

## How to Build

### Linux:

#### Build fshlvmllvmwrapper and fshlvmruntime shared library

```
cd $FSHLVM_PATH/src/FsHlvm.Runtime
make
make install
```

It will copy the fshlvmllvmwrapper.so and fshlvmruntime.so to $FSHLVM_PATH/lib

#### Build FsHlvm

```
sh build.sh
```

Alternative method:  
Linux/mono: open the FsHlvm.sln project file with Monodevelop and build the project. This will generate the FsHlvm.Core.dll assembly for you.

### OS X

Not yet tested on OS X.

### Windows (32bit), using msbuild

Works in progress, only the LLVM codegen are working.

#### Build fshlvmllvmwrapper and fshlvmruntime shared library

Works in progress.

#### Windows (32bit)

#### Build FsHlvm

```
build.cmd
```

Alternative method:  
Windows: open the FsHlvm.sln project file with Visual Studio, Xamarin Studio or Monodevelop, 
change the configuration to Release|x86 and build the project.  
This will generate the FsHlvm.Core.dll assembly for you.  

## Development Notes

### Using FsHlvm in your project

In order to use FsHlvm you will want to check the following:

1. Example F# code under FsHlvm.Main.  
2. Tests F# code under FsHlvm.Core.Tests.  

### Editing the Project with Visual Studio, Xamarin Studio or MonoDevelop

Open `FsHlvm.sln`, and edit in modes Debug or Release. 

## How to Test and Validate

### Linux 

After building run
```
cd $FSHLVM_PATH/bin
ln -s ../lib/libfshlvmllvmwrapper.so .
ln -s ../lib/libfshlvmruntime.so .
mono Release/FsHlvm.Main.exe
opt-3.4 -tailcallelim -std-compile-opts < list.bc >listopt.bc
clang -o listopt listopt.bc -ldl
./listopt
```

After building test (each test must run individually, one by one!)

```
cd $FSHLVM_PATH/tests/FsHlvm.Core.Tests
ln -s ../../lib/libfshlvmllvmwrapper.so .
ln -s ../../lib/libfshlvmruntime.so .
./run.sh -run=KPTech.FsHlvm.Core.Tests.FsHlvmTest+applicationTests.boehm
opt-3.4 -tailcallelim -std-compile-opts < bin/Release/boehm.bc >boehmopt.bc
clang -o boehmopt boehmopt.bc -ldl
./boehmopt
```

### Windows (32bit)

After building run
```
cd %FSHLVM_PATH%\bin
copy %LLVM_PATH%\LLVM-3.4.dll libLLVM-3.4.so
x86\Release\FsHlvm.Main.exe
```