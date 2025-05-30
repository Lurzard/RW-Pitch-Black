PB was made with usage of the SlugBase mod template, you can use it too [here](https://github.com/SlimeCubed/SlugTemplate)!

This is for the development of Rain World: Pitch Black, a mod for Rain World (Requires DLC).

Basegame and Watcher have been a huge inspiration in recent times

Started development 04/07/2023
Beta released 09/15/2023
Resuming mod dev after a long hiatus on 03/28/2025

-Lurzard

#Code Style Guide
- Make sure variable names make sense for what they are used for. No random single-letter variables. The only exception to this is if you are pattern matching in an 'if' statement and immediantly use the single-letter variable to pattern match to another variable that has a full name.
- If you are making a conditional block of code, use curly braces to contain it and make the code that is inside the conditional on a new line. If you have multiple related conditional statements (like more than 4), that are one line, then it is fine to put them on the same line right after the if statement.
- Try to reduce making new classes and .cs files. This one isn't as important, but it could help reduce the amount we have (it feels like we already have a few). But basically if a hook or class is relevent somewhere else, put it there before creating a new file.
- Do NOT use static global variables for things related to slugcat abilities. This causes them to have errors when more than one of ours is active at once.
- If you must use static global variables for anything, put them in the Plugin.cs file, in the Plugin class, with all the others.
- You can put the curly brace containing contitional code on it's own line, or right after the if statement that triggers it. Either is fine, but the latter is prefered. Just be consistent in how you do it.
- When naming variables, use camelCase. That is, the name starts with lower case, and then each new word is uppercase. soThisIsAnExampleOfThat.
- Comments should get their own line, right above the thing they are refering to. So no trailing comments on the same line as code.
- Comments for documentation should have a space between //+text, but for commented-out lines of code, do not include a space.
- Use comments.
- Try to make proper use of private/static/internal keywords.