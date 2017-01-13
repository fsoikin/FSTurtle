# Turtle graphics

![Travis Build Status](https://travis-ci.org/fsoikin/FSTurtle.svg?branch=master)
[![AppVeyor Build Status](https://ci.appveyor.com/api/projects/status/sifnaq8dmm1sc867/branch/master?svg=true)](https://ci.appveyor.com/project/fsoikin/fsturtle/branch/master)

Hosted at http://fsturtle.azurewebsites.net

## Possible commands
* penUp()
* penDown()
* move x y
* color "red"

## Example

```

let print_h() =
  penDown()
  move 0 100
  move 0 -50
  move 50 0
  move 0 -50
  move 0 100
  penUp()
  move -50 -100

let print_i() =
  penDown()
  move 50 0
  move -25 0
  move 0 100
  move -25 0
  move 50 0
  penUp()
  move -50 -100 

move -70 0
print_h()
move 70 0
print_i()
```
