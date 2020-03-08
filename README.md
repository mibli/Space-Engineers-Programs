# Space Engineers Programs

## Miner

Simple cone miner controller, that spins two pistons with 4 horizontal
extensions and 4 vertical erections (16 total). You can tweak the numbers in the
script.

### The build is:

* 'Drill Rotor - Rotor (Max, min angle is respected, and probably required)
* 'Drill Extender' - Horizontal Piston (min to max)
* 'Drill Erector' - Vertical Piston (min to max)

The program looks up the pistons by the names given above

### Arguments:

* "reset" - force search for actuators and reset rotor and extender to min

### States:

* "reset" - program has been executed with "reset" argument, or new piston has
  been discovered on "maxreach" state, thus the drill is taversing towards min
  position
* "rotatning" - rotor is spinnig from extreme to extreme (most common state)
* "extending" - extender is extending over the next step distance
* "erecting" - erector is extending over the next step distance
* "retracting" - exteder is retracting to min position after erecting to new
  layer)
* "maxreach" - extender and erector are on max position, so the
  script cant proceed
* "resetting" - rotor and extender are taversing towards min position
* "unknown" - if state is not set, it's assigned this state
