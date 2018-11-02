GlobalGameEvent

examples:
- tick 20 - snake - 3 - turn - left
- tick 30 - snake - 2 - eatApple - (3, -71)


do we even need an eat apple event?
i think we need it for rollback
what if we knew where all the apples would be for the entire game given any tick and at what point they should be active

AllApplesState:
- position
    - spawnTick
    - eatenTick

You don't even have to generate it ahead of time if you can make a pure function that takes in the tick and returns the spawn position




Snake
- DoTick
    - check if there is a possible apple at head position
    - if yes, has it spawned?
    - if yes, has it been eaten?
    - if no, eat it
        - marke apple as eaten in AllApplesState, and add snake tail
- RollbackTick
    - check if there is a possible apple at head position
    - if yes, has it spawned?
    - if yes, was it eaten on this tick?
    - if yes, reverse eating it
        - marke apple as not eaten in AllApplesState, and remove snake tail
