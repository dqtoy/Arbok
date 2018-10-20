PHASE ARBOK
===========

# build original snake first
- single player
- simple
- arrow keys to move

## TODO
- [x] no 108 turns
- [ ] spawn apples
- [x] make apples disappear
- [x] collide with self
- [ ] change direction immediately on keypress
- [x] 2 player snake (AI?)
    - [x] snakes collide with eachother
- [ ] border slowly shrinking, or falling away
- [x] move camera with snake
- [x] zoom out camera based on snake length
- [x] sync apples on join match
- [ ] cleanup on leave match
    - [ ] camera
    - [ ] apples
    - [ ] snakes
- [ ] support leaving and rejoining a match
    - [ ] camera
    - [ ] apples
    - [ ] snakes
- [ ] move one tick at a time on keypress
- [ ] sync head direction on join
## Possible Issues
- Apple syncing
    - if snake eats an apple or something right when a new player joins, that apple might still show as active on the new players client

