Feature: Arbok

    First phase

    Scenario: Player launches game
        When I launch the game
        Then I see a text box for my name and a play button
        Then I can click play w/o setting a name and I will be given a random name
        Then I see loading screen
        Then then im a snake in the main game
        Then my snake is a random color/texture, but unique for the match that I'm
        Then (spawned near the edge)
        Then my snake starts moving in a random direction (up|right|down|left)
        Then camera follows head of snake loosely
        Then camera zooms out proportional to snake length
        Then i see another snake, they are a different color than me
        Then we go towards eachother

    Scenario: Two snakes collide head on
        Given 2 snakes heading directly towards eachother
            """
            ooo---ooo
            ooo--ooo
            """
        When they collide
        Then they both die

    Scenario: Final two snakes collide head on
        Given 2 snakes heading directly towards eachother
        And they are the last 2 players in the match
        When they collide
        Then they both die
# Longer snake wins?
# Draw?


