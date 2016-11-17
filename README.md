# Turtle graphics in F#

![Build Status](https://travis-ci.org/fsoikin/FSTurtle.svg?branch=master)

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
  penUp()
  move 0 -50
  penDown()
  move 50 0
  penUp()
  move 0 -50
  penDown()
  move 0 100
  penUp()
  move -50 -100

let print_i() =
  penDown()
  move 50 0
  penUp()
  move -25 0
  penDown()
  move 0 100
  penUp()
  move -25 0
  penDown()
  move 50 0
  penUp()
  move -50 -100

move -70 0
print_h()
move 70 0
print_i()
```
