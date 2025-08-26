# Linear programming basics
A
 short explanation is given what Linear programming is and some basic knowledge you need to know.

A linear programming problem is mathematically formulated as follows:

- A **linear function to be maximized or minimized**:
  > maximize $\quad c1*x1 + c2*x2$
- **Problem constraints** of the following form
  > $\begin{aligned} a11*x1 + a12*x2 \leq b1 \\ a21*x1 + a22*x2 \leq b2 \\ a31*x1 + a32*x2 \leq b3 \end{aligned}$
- **Default lower bounds of zero** on all variables.

The problem is usually expressed in matrix form, and then becomes:
  > $\begin{aligned}\text{maximize}\quad &C^T x \\\text{subject to}\quad &A x \leq B \\ &x \geq 0\end{aligned}$

So a linear programming model consists of one objective which is a **linear** equation that must be maximized or minimized.
Then there are a number of **linear** inequalities or constraints.

$c^T$, $A$ and $B$ are constant matrixes.
$x$ are the variables (unknowns).
All of them are real, continue values.

Note the default lower bounds of zero on all variables $x$.
People tend to forget this built-in default.
If no negative (or negative infinite) lower bound is explicitely set on variables, they can and will take only positive (zero included) values.

The inequalities can be $\leq$, $\geq$ or $=$
Because all numbers are real values, $<=$ is the same as $<$ and $>=$ is the same as $>$.

Also note that both objective function and constraints must be linear equations.
This means that no variables can be multiplied with each other.

This formulation is called the _Standard form_.
It is the usual and most intuitive form of describing a linear programming problem.

Example:

```
minimize     3 x1 -   x2
subject to    -x1 + 6 x2 - x3   + x4 >= -3
                    7 x2      + 2 x4  =  5
               x1 +   x2 + x3         =  1
                           x3 +   x4 <=  2
```

Sometimes, these problems are formulated in the _canonical_ form.
All inequalities are converted to equalities by adding an extra variable where needed:
  > $\begin{aligned}\text{maximize}\quad &C^T x \\\text{subject to}\quad &A x \leq B \\ &x \geq 0\end{aligned}$

Above example can then be written as:

```
minimize     3 x1 -   x2
subject to    -x1 + 6 x2 - x3   + x4 - s = -3
                    7 x2      + 2 x4     =  5
               x1 +   x2 + x3            =  1
                           x3 +   x4 + t =  2
```

So everywhere an equality was specified, an extra variable is introduced and subtracted (if it was >) or added (if it was <) to the constraint.
These variables also only take positive (or zero) values only.
These extra variables are called _slack_ or _surplus_ variables.

lp_solve adds these variables automatically to its internal structure.
The formulator doesn't have to do it and it is even better not to.
There will be fewer variables in the model and thus quicker to solve.

See [Formulation of an LP problem](./formulate-problem.md) for a practical example.

The right hand side (RHS), the B-vector, must be a constant matrix.
Some people see this as a problem, but it isn't.
The RHS can always be brought to the left by a simple operation:

```
A x <= B
```

is equal to:

```
A x - B <= 0
```

So if B is not constant, just do that.
Basic mathematics also states that if a constraint is multiplied by a negative constant, that the inequality changes from direction. For example:

```
5 x1 - 2 x2 >= 3
```
If multiplied by -1, it becomes:

```
-5 x1 + 2 x2 <= -3
```

If the objective is multiplied by -1, then maximization becomes minimization and the other way around. For example:

```
minimize     3 x1 - x2
```

Can also be written as:

```
maximize     -3 x1 + x2
```

The result will be the same, but changed from sign.

## Bounds

Minima and maxima on single variables are special cases of restrictions.
They are called bounds.
The optimization algorithm can handle these bounds more effeciently than other restrictions.
They consume less memory and the algorithm is faster with them.
As already specified, there is by default an implicit lower bound of zero on each variable.
Only when explicitly another lower bound is set, the default of 0 is overruled.
This other bound can be negative also.
There is no default upper bound on variables.
Almost all solvers support bounds on variables.
So does lp_solve.

## Ranges

Frequently, it happens that on the same equation a less than and a greater than restriction must be set.
Instead of adding two extra restrictions to the model, it is more performant and less memory consument to only add one restiction with either the less than or greater than restriction and put the other inequality on that same constraint by means of a range.
Not all solvers support this feature but lp_solve does.

## Integer and binary variables

By default, all variables are real.
Sometimes it is required that one or more variables must be integer.
It is not possible to just solve the model as is and then round to the nearest solution.
At best, this result will maybe furfill all constraints, but you cannot be sure of.
As you cannot be sure of the fact that this is the most optimal solution.
Problems with integer variables are called integer or descrete programming problems.
If all variables are integer it is called a pure integer programming problem, else it is a mixed integer programming problem.
A special case of integer variables are binary variables.
These are variables that can only take 0 or 1 as value.
They are used quite frequently to program discontinue conditions.
lp_solve can handle integer and binary variables.
Binary variables are defined as integer variables with a maximum (upper bound) of 1 on them.
See [integer variables](https://lp-solve.github.io/integer.htm) for a description on them.

## Semi-continuous variables

Semi-continuous variables are variables that must take a value between their minimum and maximum or zero.
So these variables are treated the same as regular variables, except that a value of zero is also accepted, even if there is a minimum bigger than zero is set on the variable.
See [semi-continuous](https://lp-solve.github.io/semi-cont.htm) variables for a description on them.

## Special ordered sets (SOS)

A specially ordered set of degree N is a collection of variables where at most N variables may be non-zero.
The non-zero variables must be contiguous (neighbours) sorted by the ascending value of their respective unique weights.
In lp_solve, specially ordered sets may be of any cardinal type 1, 2, and higher, and may be overlapping.
The number of variables in the set must be equal to, or exceed the cardinal SOS order.
See [Special ordered sets (SOS)](https://lp-solve.github.io/SOS.htm) for a description on them.

lp_solve uses the simplex algorithm to solve these problems.
To solve the integer restrictions, the branch and bound (B&B) method is used.

## Other resources

Another **very useful** and free paper about linear programming fundamentals and advanced features plus several problems being discussed and modeled is [Applications of optimization with Xpress-MP](http://dashoptimization.com/home/downloads/book/booka4.pdf).
It describes linear programming and modeling with the commercial solver Xpress-MP, but is as usefull for other solvers like lp_solve.
In case that this link would not work anymore, try via [this google search](http://www.google.be/search?hl=nl&as_qdr=all&q=%22Applications+of+optimization+with+Xpress-MP%22+%22Developing+Linear+and+Integer+Programming+models%22+%22Application+examples%22+filetype%3Apdf&btnG=Zoeken&meta=).