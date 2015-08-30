# Skyling

Skyling is a contextualization library for C# code, meant to assist in building intelligent developer-focused tools. Giving it a code-base, it'll break down individual expressions, blocks, methods and classes into a form that can then be easily compared, combined or analyzed. Alongside this it will also label each section of code with a human understandable meaning, such as 'validation', 'credit card processor', 'web service', 'dictionary', 'string concatenation'. 

This has multiple potential applications:

* Automatic refactoring of code and architectural enforcement. Rules can be built that mean code and API's should be clustered together based on a set of labels, and being able to compare before/after to make sure logic hasn't changed.
* Code synthesis from human understandable words. Saying you want a 'web service' for 'books' that 'validates' based on 'title' will give itenough information to combine existing code into a new form that satisfies your requirements.
* Optimization of existing code. Attaching ratings to each block of code allows multiple forms to be synthesized together to find one with the most optimal rating, depending what you need. Logical comparison can then be used to make sure it still works exactly the same.

This will be experimental for a long time yet.

## Get Involved

If you're interested in getting involved, feel free to contact me or play around with the code yourself. The reposiztories documentation will be built up incrementally as time goes on, so apologies if things may be missing. They'll be there eventually.