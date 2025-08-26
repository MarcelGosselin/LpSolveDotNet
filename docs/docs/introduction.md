# Introduction

[LpSolveDotNet](https://github.com/MarcelGosselin/LpSolveDotNet) is a library aiming to help you build a mathematical model in .NET and **solve it** using the open source Mixed Integer Linear Programming (MILP) solver [lp_solve](https://github.com/lp-solve/lp_solve) which is itself written in C. 

> [!TIP]
> Although this documentation has copied/adapted parts of the [official lp_solve documentation](https://lp-solve.github.io/),
> not all of it has been copied/adapted and so there is still a lot of useful information over there.
>
> Whenever you look at their documentation and you see functions, remember that [LpSolveDotNet.LpSolve class](xref:LpSolveDotNet.LpSolve) has the same.
> - Any function having a first parameter `lprec *lp` is a member method of the class where that parameter is removed because it is passed implicitly instead.
> - All other functions are static methods on the class.

## Types of Solvable Mathematical Models

The types of models this library can solve are:
- pure linear
- (mixed) integer/binary
- semi-continuous
- special ordered sets (SOS)

According to lp_solve's own documentation:
> It is a free (see LGPL for the GNU lesser general public license) linear (integer) programming solver
based on the revised simplex method and the Branch-and-bound method for the integers.
It contains full source, examples and manuals.
lp_solve solves pure linear, (mixed) integer/binary, semi-continuous and
special ordered sets (SOS) models.

## Quick Start

To start using LpSolveDotNet:
1. You **must** [set up your project](./setting-up-project.md)
1. If you don't know much about LP modelling you **could** [learn linear programming basics](./linear-programming-basics.md)
1. You should [see how to formulate a problem in LpSolveDotNet](./formulate-problem.md). The _Mathematical and Graphical Solution_ tab is more for those who don't know much about LP modelling, but the _LpSolveDotNet Solution_ tab is for anyone.
1. [Familiarize yourself with basic concepts](./basic-concepts.md)
1. [Look at the LpSolve class API](xref:LpSolveDotNet.LpSolve)
