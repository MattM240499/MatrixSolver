# Vector Reachability Solver
This program solves the Vector Reachability problem based on a file containing vectors x, y and a list of matrices M1, ..., Mn.

## Prerequisites
- You must install the .NET framework version 3.1 or higher.
## How to Vector Reachability Problem solver
First create a json file with the following properties

- VectorX - An array of length 2
- VectorY - An array of length 2
- Matrices - A 3d array of length n x 2 x 2. In other words, a list of 2x2 matrices.

The matrices specified must be 2x2 matrices and they must have determinant 1. VectorX and VectorY must have the same determinant.

Then to run the program use the following command in the ```src/MatrixSolver``` folder: 
```
    dotnet run <filepath>
```

## Examples

You can find examples in the ```src/MatrixSolver/Examples``` folder.

For example, you can run a simple example with the command from inside the ```src/MatrixSolver``` folder.
```
    dotnet run Examples/SimpleExamples/SimpleExample1.json
```

## How to use tools
Tools can be located in the ```tools``` folder. The VRP verifier tool can be found in the ```tools/VectorReachabilitySolutionChecker```, and The Matrices to words tool can be found in the ```tools/CalculateManyMatricesFromWords``` folder. To run each program use the following command in the respective folders.
```
    dotnet run <filepath> <word>
```



## How to run tests
In the root directory, run the following command
```
    dotnet test
```

