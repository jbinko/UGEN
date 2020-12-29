# UGEN - Utterances Generator

## Overview

UGEN is command line tool which generates labeled utterances from input file based on specifically designed combinatorial language.
This project is inspired by PUTPUT tool described here: <https://github.com/michaelperel/putput>

UGEN can help you generate many testing and/or example utterances for some specific domain space.
Generated samples can be stored as JSON files and can be used by LUIS engine.
<https://docs.microsoft.com/en-us/azure/cognitive-services/luis/>
JSON is produced in LUIS Batch file format
<https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-concept-batch-test#batch-file-format>.

UGEN rules allow you to specify intents and entities (via metadata) so you can better describe utterances and meanings.  

### When to use this project?

This tool can help you provide and generate labeled data for machine learning models,
chat bot conversations, provide utterances with structure and patterns.
Any time when you need to have large amount of sample data related to NLP models
you might benefit from this open source tool.

### Features

- Flexible and easy to use pattern language describing rules/patterns and combinatorial logic
- Supports rules, automatic dependencies, intents and entities to label data
- Generates LUIS JSON compatible file with utterances annotated with intents and entities including start/end positions
- Can print expanded rules/utterances to validate intermediate or complete results on the screen
- Specifically designed language and crafted parser for parsing rules (based on ANTLR 4.0 grammar)
- Language supports recursion, combinatorial operators (comma and pipe), optional rule evaluation operator question mark and parenthesis to specify rules priority 
- Parser and model built in validation (errors and warnings) for unused rules, missing rules references, cyclic rules dependencies, etc.
- Open source tool released under permissive free software MIT license. Anybody can benefit, anybody can contribute, no commercial restrictions.

### How to use

You need to create simple text file with set of rules in UGEN language describing utterances patterns.
Next, use the UGEN tool to generate LUIS engine compatible output JSON file
or just print calculated results on the screen.
UGEN will read specified rules, will understand dependencies between rules, will combine rules together with combinatorial
logic and will store results - including metadata if provided.

This will allow you quickly with only few rules generate large amount of samples for your specific domain space.
