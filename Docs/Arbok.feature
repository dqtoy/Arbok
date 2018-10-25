Feature: Arbok

    First phase

    Scenario: Player launches game
        When I launch the game
        Then I see a text box for my name and a play button
        Then I can click play w/o setting a name and I will be given a random name
        Then I see loading screen (looking for game)
        Then then im a snake in a lobby with other snakes
        And It is not possible to die
        And I can see text saying how many players are in the game
        When the minimum player count is reached
        Then there is text that says "game is starting in 3...2..1.."
        Then my snake is moved to its starting location (near the edge)
        And my snake is a random color/texture, but unique for the match that I'm
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

    Scenario: Player wins a match
        Given there are 2 snakes alive in the match
        And the other snake dies before me
        Then I am the winner
        And I see text that says "WINNER WINNER SNAKE DINNER?"
        And there are some stats about the game displayed
        And there is a quick replay of the entire match in the background (bonus)
        And there is a spectate button
        And there is a play again button
        And there is a main menu button

    Scenario: Player dies early in match but spectates until the game is over
# Longer snake wins?
# Draw?


lobby or waiting room
or just show status text until the game actually starts
