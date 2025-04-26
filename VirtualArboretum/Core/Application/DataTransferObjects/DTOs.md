# Data Transfer Objects

..sind reine Datenübertragungsobjekte,  
die zur Infrastrukturschicht gehören  
und die Kommunikation zwischen Schichten  
oder Systemen erleichtern.

Also Helper **ohne Domänenlogik**!

 - Should never contain any internal Domain-logic!
 - Should contain serialised Variants of your Domain-logic.
 - Should only contain LANGUAGE PRIMITIVES for external interaction.
   - De-/Serialization is being handled in either Use-Case or Domain-Model itself.