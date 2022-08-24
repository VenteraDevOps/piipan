#!/usr/bin/env python3

# Python code convert hex string
# to the Base64 string 
  
import codecs
import sys

hex = sys.argv[1]
b64 = codecs.encode(codecs.decode(hex, 'hex'), 'base64').decode()

# Print the resultant string
print (str(b64))