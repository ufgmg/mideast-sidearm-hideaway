![Mideast Sidearm Hideaway](http://lumeh.org/files/public/mideast_sidearm_hideaway.png)

## Game description

### Backstory

You are a lone space traveler whose ship has been impaired. Unfortunately, the
parts you need to repair your ship have fallen into the black hole, and you
have no way to get them out.

### Gameplay synopsis

Kill dangerous enemies that are trying to kill you, or trick them into being
sucked into the black hole. If enough things fall into the black hole, it will
violently eject its contents, giving you a chance to recover the ship parts
you need.

### Player mechanics

*   Move (WASD)
*   Aim (mouse)
*   Shoot (left button)
*   Grappling hook grab/release (right button)
*   Multi-dimensional anchoring boots toggle (spacebar)

#### Weapons

##### Projectile

*   Flame thrower
*   Gatling gun
*   Gravity well
*   Rocket launcher

##### Beam

*   Freeze ray
*   Laser

##### Melee

*   Chainsaw
*   Plank with nail

### Level mechanics

You'll be knocking enemies into the black hole, either by tricking them
into the black hole or by killing or otherwise paralyzing them so that
they naturally fall into the black hole. Once enough things fall into
the black hole, the level ends.

#### Food cart

The food cart shows up along a side of the screen from time to time.
You can purchase health-restoring food and possibly even weapons and such
from it. You spend points (score points), and the prices are somewhat
proportional to your score.

#### Space levels

*   Black hole somewhere on the border that sucks things toward it
*   Enemies spawning around the border of the screen. See types of enemies
    below.

#### Non-space levels ??

### Scoring

*   Style points for killing enemies awesomely
*   A certain amount of stuff needs to fall into the black hole

### Enemies

Enemy collision: yes, Starcraft-flying-unit-type collision while the
enemies are active; no collision once they are dead.

#### Goblin (in space suits?)

The typical, basic grunt unit.

#### Leprechaun

Ranged unit that attacks by throwing coins at you.

#### Unicorn

Relatively neutral unit that appears on a side of the screen, prepares, and
then charges across the screen, damaging anything in his path and leaving
behind a temporary rainbow bridge that can be utilized to quickly travel
across the screen. It may or may not be possible to latch onto one with your
grappling hook, and even if it is, the effects may be deadly...

#### Ogers

Heavy unit that has a slight gravitational pull and possibly also a large
attack that can dangerously launch other enemies at you.

## How to work on code for the game

### Initial setup

You have two options for contributing:

*   Push your branches directly to this repository.
*   Push your branches to your own fork.

I'll explain the case where you push directly to this repository:

1.  Be a contributor for this repository. Send me a message; I'll add
    you.

2.  Clone this repository. This will automatically set you up with a
    remote called `origin` that points here.

3.  Delete your local `master` branch, so that you won't accidentally
    push to master:

        $ git checkout origin/master
        $ git branch -d master

### Working on a feature

1.  Make sure your remote tracking branches (branches named `origin/*`) are
    up to date:

        $ git fetch origin

2.  Start a new feature branch based on `origin/master`.

        $ git checkout -b newbranchname origin/master

3.  Start coding! Commit often! Don't worry about making too many
    commits; we can combine them into fewer commits later.

4.  If there are new commits on the repository that you want to
    incorporate into your feature branch, fetch and then rebase:

        $ git fetch origin
        $ git rebase origin/master

5.  Push your commits to the repository:

        $ git push origin branchname

### Other useful Git commands

    # Display useful status about the current state of your branch
    $ git status

    # Display the commit log for your branch
    $ git log

    # Stage changed files
    $ git add <file>

    # Commit staged changes
    $ git commit -m "<commit message>"

    # List all branches on your computer
    $ git branch -a

    # Check out a branch
    $ git checkout

    # Make a new branch
    $ git checkout -b <new branch name> <base branch name>

    # Download updates from a remote repository
    $ git fetch <remote name>

    # Rebase your current branch on top of another branch
    # This is most likely what you should do if you have made some commits
    # on your branch but there are new commits in the master repository that
    # you want to include.
    $ git rebase <branch name>
