#!/usr/bin/env python3

# Python code convert calculate sha256 sum 
  
import codecs
import sys
import hashlib

hex = sys.argv[1]

value=codecs.decode(hex, 'hex')
shaResult=hashlib.sha256(value)

# Print the resultant string
print (shaResult.hexdigest())