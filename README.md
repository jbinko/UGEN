# UGEN - Utterances Generator

## Overview

UGEN is command line tool which generates labeled utterances from input file based on specifically designed combinatorial language.
This project is inspired by PUTPUT tool described here: <https://github.com/michaelperel/putput>

UGEN can help you generate many testing and/or example utterances for some specific domain space.
Generated samples can be stored as JSON files and can be used by LUIS engine.
<https://docs.microsoft.com/en-us/azure/cognitive-services/luis/>
Produced JSON file is in LUIS Batch compatible file format
<https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-concept-batch-test#batch-file-format>.

UGEN language rules allow you to also specify intents and entities metadata so you can better describe utterances and meanings.  

### When to use this project?

This tool can help you provide and generate labeled data for machine learning models,
chat bot conversations, provide utterances with structure and patterns.
Any time when you need to have large amount of sample data related to NLP models
you might benefit from this open source tool.

### Features

- Flexible and easy to use pattern language describing rules/patterns and combinatorial logic
- Supports rules, automatic dependencies, intents and entities to label data
- Generates LUIS JSON compatible file with utterances annotated with intents and entities including start/end entities positions
- Can print expanded rules/utterances to validate intermediate or complete results on the screen
- Specifically designed language and crafted parser for parsing rules (based on ANTLR 4.0 grammar)
- Language supports recursion, static lists of strings in square brackets,
    combinatorial operators (comma and pipe), optional rule evaluation operator question mark and parenthesis to specify rules priority.
- Parser and model built in validation (errors and warnings) for unused rules, missing rules references, cyclic rules dependencies, etc.
- Open source tool released under permissive free software MIT license. Anybody can benefit, anybody can contribute, no commercial restrictions.

### How to use

You need to create simple text file with set of rules in UGEN language describing utterances patterns.
Next, use the UGEN tool to generate LUIS engine compatible output JSON file
or just print calculated results on the screen.
UGEN will read specified rules, will understand dependencies between rules, will combine rules together with combinatorial
logic and will store results - including metadata if provided.

This will allow you quickly - with only few rules generate fair amount of sample data for your specific domain space and language understanding project.

## Examples

Following example is demonstrating separation to different rules with names (including intent).
Showing static texts specification in lists E.g. [Hi, Hey, ...].
Showing optional operator ? (items are combined together with and without results of expressions marked with ? operator).
E.g. Greeting intent can expand to simple HI texts or can expand up to more complex utterances including GOOD_DAY, BOT, HOWRU.
Operator | - items are combined together with and without results in exclusive way.
It expands left side with others exclusively and then right side with others exclusively.
Finally, operator comma , (Not in static lists) is simply appending/combining items represented by expressions Together.
E.g. HI, BOT expression will join all items represented by static lists HI and BOT Together.

```
intent Greeting
: ( HI | GOOD_DAY )? , BOT?, HOWRU?

HI
: [Hi, Hey, Hello, Hi ya, Dear, Howdy, Greetings]

BOT
: [bot, agent, IT, assistant, operator, representative, worker, delegate]

HOWRU
: [how are you, how goes it, what's happening, what's up]

GOOD_DAY
: [Good]?, [day, morning, afternoon, evening]
```

Use example rules above and store them to text file ```test.ugen```.
Running ugen tool like this  ```./ugen print test.ugen``` will produce results shown below (result is abbreviated for clarity).

```
Producing rule: HI
Producing rule: GOOD_DAY
Producing rule: BOT
Producing rule: HOWRU
Producing rule: Greeting
Rule 'HI':
  Hi
  Hey
  Hello
  ...
Rule 'GOOD_DAY':
  Good day
  Good morning
  day
  morning
  ...
Rule 'BOT':
  bot
  agent
  ...
Rule 'HOWRU':
  how are you
  how goes it
  ...
Rule 'Greeting' (Intent):
  Hi bot how are you
  Hi bot how goes it
  Hi bot what's happening
  Hi bot what's up
  Hi agent how are you
  Hi agent how goes it
  Hi agent what's happening
  Hi agent what's up
  Hi IT how are you
  Hi IT how goes it
  Hi IT what's happening
  Hi IT what's up
```

You can also produce JSON file with more rich and useful information. You can try this command:
```./ugen create test.ugen -f -o test.json```
