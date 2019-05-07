This is an initial test run for incorporating Firebase Database and Firebase Storage into Unity ARCore.

At a high level, this code finds all of the items in a given database, instantiates a GameObject for each of them, and assigns to
that GameObject the relevant data from the Database.

For each object in our Database, we store a unique ID mapping to a position as an array floats.

This code also sets up the basic structure to retrieve audio from our Firebase Storage given a unique string
and have it start playing from an AudioSource nested to the corresponding GameObject.
