PHASE ARBOK
===========

# build original snake first
- single player
- simple
- arrow keys to move

## TODO
- [x] no 108 turns
- [x] spawn apples
- [x] make apples disappear
- [x] collide with self
- [ ] change direction immediately on keypress
- [x] 2 player snake (AI?)
    - [x] snakes collide with eachother
- [x] border slowly shrinking, or falling away
- [x] move camera with snake
- [x] zoom out camera based on snake length
- [x] sync apples on join match
- [x] cleanup on leave match
- [x] support leaving and rejoining a match
- [x] move one tick at a time on keypress
- [x] sync head direction on join
- [ ] interpolation of snake movement
- [ ] playback game after game ends
- [x] make snake death a reversible event
- [ ] maybe sync entire event list of snakes when joining in progress game?
- [ ] make way to prioritize different event types like snake vs apple
- [ ] store positions for snakes and apples in 2D array, first key is x position, second key is y, O(1)
- [ ] export entire event lists to json and compare in diff tool
- [ ] more hotkeys to navigate event list and pause ticks on all clients
- [ ] make apple spawning always instantiate or destroy apples, don't deactivate them
- [ ] make apple spawn use a deterministic RNG so you dont have to send apple spawn events over the server (perlin?)
- [ ] use animation to hide latency issues
- [ ] do proper lockstep with client side prediction
    - every client sends player actions every tick + last 10 ticks, unreliably
    - when new client joins, server sends game state at current tick
    - then the client knows exactly what tick it's at and can be sure that it hasn't missed anything, and it knows what events from what players it needs from the future
- [ ] queue up turn events to allow doing a quick 180

## Possible Issues
- Apple syncing
    - if snake eats an apple or something right when a new player joins, that apple might still show as active on the new players client

## Network Bugs
- one snake is longer than its remote version
- positions are off by one sometimes


## Server Start
- [ ] wait for players
- [ ] start countdown once enough players
- [ ] start game at end of countdown

## Host Server
- snake spawn

## Join Server
- snake spawn
- other snakes updated
- apples updated
- GlobalTick in sync with server
- turn on snake
