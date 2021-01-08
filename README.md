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
- Supports rules, automatic tracking of dependencies between rules, intents and entities to label data
- Generates LUIS JSON compatible file with utterances annotated with intents and entities including start/end entities positions
- Can print expanded rules/utterances to validate intermediate or complete results on the screen
- Specifically designed language and crafted parser for parsing rules (based on ANTLR 4.0 grammar)
- Language supports recursion, static lists of strings in square brackets,
    combinatorial operators (comma and pipe), optional rule evaluation operator question mark and parenthesis to specify rules priority.
- Parser and model built in validation (errors and warnings) for unused rules, missing rules references, cyclic rules dependencies, etc.
- Open source tool released under permissive free software MIT license. Anybody can benefit, anybody can contribute, no commercial restrictions.

## Prerequisites to run this application

In short - NO any prerequisites exists. This project is built on and using .NET 5.0 (.NET Core).
If you are planning just to use compiled multi platform binaries which are part of the release build, you DO NOT need to install anything (including .NET runtime).
All what you need is included in the release build.

### Prerequisites to build this application

If you are planning to build this project from a source code, you need to deploy .NET 5.0 SDK.
You can download the .NET 5.0 SDK here: <https://dotnet.microsoft.com/download/dotnet-core>. Select target platform and SDK version.

## Installation

Download the latest release version of this project from the releases web page: <https://github.com/jbinko/UGEN/releases>.
Following files are available in assets section:

- `UGEN.zip` - contains binary files which needs to be unzipped on target machine
- `Source code.zip` - Source code of this project
- `Source code.tar.gz` - Source code of this project in different tar/zip format

- Download the `UGEN.zip` file. Extract the content of the zip file into any preferred directory from which you will run this tool.
- Try to run the tool with command: `./ugen -h`. It should show you all the available options of the command line tool.

## How to use this tool

You need to create simple text file with set of rules in UGEN language describing utterances patterns.
Next, use the UGEN tool to generate LUIS engine compatible output JSON file
or just print calculated results on the screen.
UGEN will read specified rules, will understand dependencies between rules, will combine rules together with combinatorial
logic and will store results - including metadata if provided.

This allows you quickly (with only few rules) generate fair amount of sample data for your specific domain space and language understanding project.

### Examples

Following example is demonstrating:

- Separation of different rules with specific names (including intent).
- Showing static texts specification in lists E.g. ```[Hi, Hey, ...]```.
- Showing optional operator ```?``` (items are combined together with and without results of expressions marked with ```?``` operator).
E.g. Greeting intent can expand to simple ```HI``` texts or can expand up to more complex utterances including ```GOOD_DAY, BOT, HOWRU```.
- Operator ```|``` - items are combined together with and without results in exclusive way.
It expands left side with others exclusively and then right side with others exclusively.
- Finally, operator comma ```,``` (Not the one in static lists) is simply appending/combining items represented by expressions Together.
E.g. ```HI, BOT``` expression will join all items represented by static lists ```HI``` and ```BOT``` Together.

```
intent Greeting
: ( HI | GOOD_DAY )? , BOT?, HOWRU?

HI
: [Hi, Hey, Hello, Hi ya, Dear, Howdy, Greetings]

entity BOT
: [bot, agent, IT, assistant, operator, representative, worker, delegate]

HOWRU
: [how are you, how goes it, what's happening, what's up]

GOOD_DAY
: [Good]?, [day, morning, afternoon, evening]
```

Use example rules above and store them into a text file ```test.ugen```.
Running ugen tool like this  ```./ugen print test.ugen``` will produce results shown below (results are abbreviated for clarity).
Focus particularly on result of the rule 'Greeting' which combines all the rules Together.

You can also produce LUIS JSON compatible file with more rich and useful information. You can try this command:
```./ugen create test.ugen -f -o test.json```

Hopefully this short example provided you an idea about how this tool works.

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

Output of the ```test.json``` file (content is abbreviated for clarity)
```
[
  ...

  {
    "text": "Good evening assistant",
    "intent": "Greeting",
    "entities": [
      {
        "entity": "BOT",
        "startPos": 13,
        "endPos": 22
      }
    ]
  }

  ...
]
```

## Things in TODO List

- Create an option to specify limit on number of maximum utterances to be exported. Default value should be 1000. LUIS will only accept up to 1000 utterances in one batch test file.
- Strings in square brackets doesn't support escape characters currently. It means, you cannot use following characters ```,``` and ```]``` as a part of the strings. Is it really blocker or nice to have?

## UGEN Language and Grammar

Valid input or input file is structured into optional comments and rules definitions.
Input needs to contain between zero and N rules (or sometimes called patterns).

Multiple rules can be root rules. No any notion of only one main rule exists nor is required.
Each rule contains definition of pattern and can also reference other rule(s) as subrule - part of rule definition.
This allows structure rules nicely in very reusable way.

Dependencies between rules are tracked automatically.
Rules, which are referencing missing rules and/or produce
circular dependencies between rules are identified and reported as errors.

Rules are supporting recursions via rules expressions during processing.

### White spaces and new lines

White space characters like spaces and tabulators and new line characters like ```\r``` and ```\n``` are ignored from processing.
This allows to organize input file in any preferred way.  

### Comments

Two kind of comments are supported:

One line comment - Two different characters can be used to mark beginning of comment until the end of line.<br/>
Everything after following characters ```//``` or ```#``` is ignored until it reaches end of a line.<br/>
For example:
```
// This is my comment 1
#  This is my comment 2
```

Block comment - It is basically C/C++ type of block comment inside sequence of characters ```/*``` and ```*/```.<br/>
Everything between ```/*``` and ```*/``` is ignored and can even span multiple lines.<br/>
For example:
```
/*
  This is my comment and can be across multiple lines
*/
```

### Rules

Every rule is described in the definition. Every rule needs to have name/identifier and definition of the rule.
Rule can reference another rule(s) by name/identifier in the rule expression inside definition of the rule.

This is syntax for a rule:<br/>
RULE-TYPE RULE-IDENTIFIER : RULE-DEFINITION

Valid name/identifier of the rule can contain English letters (upper/lower), English numbers, underscore and hyphen characters.
Must be at least one character long and first character must be letter or underscore. Not number or hyphen.

Identifiers are case SENSITIVE during processing and if used in references it must have exactly
the same name including case - case SENSITIVE in references in other rules.

In other words - you can use upper/lower cases or mix of cases but always needs to be typed exactly the same.
Otherwise references will not be able to resolve referenced rules or will NOT reference correct rules.

For example RULE1, Rule1, rule1, rULE1 are all valid identifers for four different rules.
Duplicities on identifiers (case sensitivity) are identified and reported as errors.

Order of rules in input is NOT important (including rules with dependencies).
Dependencies are resolved automatically NO matter where they are located.

Rule can optionally have specifier for a type of the rule - It is metadata describing the rule.
Rule can describe Intent, Entity or can be just plain rule without special meaning.

You can use key words 'Intent' or 'Entity'. It is NOT case sensitive. If not specified - plain rule type is used as default.

Intent and Entity metadata is important in combination with LUIS and if you need to store metadata in result in
LUIS Batch compatible file format.
For more details see:
<https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-concept-batch-test#batch-file-format>.

Rule definition is composition of rule expression(s).
Expression can contain combinatorial operators, priority operators, strings, rule references, etc.
Expressions can also contain expressions in recursive way.

### Strings, String Literals and String Tuples

String or multiple strings are always hard-coded inside square brackets ```[``` and ```]```.
No quotes are needed and are NOT supported. They will become part of the string if they are used.

String/Strings/String tuples are always defined in some rule. They can be referenced via rule name from other rules.

String tuples are representing set of strings inside square brackets separated by comma.
Anything inside square brackets is considered to be string. If comma is found, it means multiple strings are defined inside square brackets - string tuple.

String/Strings/String tuples can be part of any rule definition expression and can be combined with other operators and expressions.

For example:
```
MY_STRING
: [Hello World]

MY_STRING_TUPLE
: [Hi, Hey, Hello World]

MY_STRING_OP1
: [Hey], [you]

MY_STRING_OP2
: [Hi] | [Hey]

MY_STRING_OP3
: [Hey]?, [You]

MY_STRING_OP4
: [Hey]? , ( [You] | [Joe] )
```

### Rule References

Rule can reference another rule(s) by name/identifier in the rule expression inside definition of the rule.
It is effectively pointer/reference to the result of another rule.
As it was described in the rules section earlier. Be aware of case sensitivity of rules identifiers and
allowed characters in the rule name/identifier.
Any dependencies between rules are automatically resolved and definition order doesn't matter.

For example:
```
GOOD_DAY
: GOOD_MY_REF, [day, morning, afternoon, evening]

GOOD_MY_REF
: [Good]
```

### Operators, Combinatorial operators

Expressions described in the rule definition can be combined with couple of supported operators.
Operators and expressions are always working with set of strings and combining strings Together somehow.
Result of those operations is again set of strings passed for further processing.
Expressions can be naturally recursive.
Operators are frequently combined Together, combined with other expressions, string tuples, other
rule references, etc.

Following is list of operators and sorted in correct operator precedence.
From the highest precedence.

1. Operator Parentheses ```( )``` - marking expression inside parentheses as to be prioritized for the processing.
2. Operator Question Mark ```?``` - marking expression it is attached to as kind of nullable.
3. Operator Pipe ```|``` - operation applied on set of strings from both sides is iteration/yield/serial/zipping operation.
4. Operator Comma ```,``` - operation applied on set of strings from both sides is ADD (concatenate) operation.

#### Operator Comma

Operator Comma ```,``` is triggering operation on set of strings coming from referenced rule or rule expression on left side of operator
and combining those strings with set of strings coming from referenced rule or rule expression on right side of the operator.

The operation applied on set of strings from both sides is ADD (concatenate) operation.
Meaning concatenate each string on left side with each string on right side and
produce new set of combined strings as result of an expression and is used for further processing.

Do not confuse this comma ```,``` operator with comma ```,``` used inside string tuples. In a string tuples this comma has
different (separator) meaning.

For example:
```
MY_RULE_01
: [Hi, Hey], [Joe, You] // Produces set of strings Hi Joe, Hi You, Hey Joe, Hey You
```

#### Operator Pipe

Operator Pipe ```|``` is triggering operation on set of strings coming from referenced rule or rule expression on left side of operator
and set of strings coming from referenced rule or rule expression on right side of the operator.

The operation applied on set of strings from both sides is iteration/yield/serial/zipping operation.
Meaning take each string on left side and put it into result and after this take each string on right side and
put it into result which is produced as new set of strings as result of an expression and is used for further processing.

You can think about it more like auxiliary operation which is useful and frequently used with previous operator.
For example:
```
MY_RULE_01
: [Hi, Hey] | [Joe, You] // Produces set of strings Hi, Hey, Joe, You
```

#### Operator Question Mark

Operator Question Mark ```?``` is marking expression it is attached to as kind of nullable.
It effectively means when combined with other operators Comma ```,``` and Pipe ```|```
during the calculation of expression it is done in two phases.
First, with all strings which are part of expression where Question Mark is attached to and second time with none strings of expression where Question Mark is attached to.
This produce new set of combined strings as result of an expression and is used for further processing.

Example will explain better:
```
MY_RULE_01
: [Hi, Hey]?, [Joe, You] // Produces set of strings Hi Joe, Hi You, Hey Joe, Hey You, Joe, You
```

#### Priority operator

Operator parentheses ```( )``` is marking expression inside parentheses as to be prioritized for the processing.
Expressions in parentheses ```( )``` take precedence over those surrounding it.
This produce new set of strings as result of an expression and is used for further processing.

For example:
```
MY_RULE_01
: [Hi] | [Joe] , [You] // Produces set of strings Hi You, Joe You

MY_RULE_02
: [Hi] | ( [Joe] , [You] ) // Produces set of strings Hi, Joe You
```
