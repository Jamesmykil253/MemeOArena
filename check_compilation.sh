#!/bin/bash
# Simple compilation check for Unity C# files

echo "Checking for common C# compilation issues..."

# Check for missing using statements
echo "=== Checking for missing using statements ==="
grep -r "UnityTestAttribute" Assets/Scripts/Tests/ && echo "Found UnityTestAttribute usage"
grep -r "UnityTest" Assets/Scripts/Tests/ | grep -v "UnityTestAttribute" && echo "Found UnityTest usage without attribute"

# Check for interface implementation issues  
echo -e "\n=== Checking interface implementations ==="
grep -r "class.*InputSource.*:" Assets/Scripts/Tests/ | while read line; do
    file=$(echo "$line" | cut -d: -f1)
    echo "Checking: $file"
    
    # Count interface methods vs implemented methods
    interface_methods=$(grep -c "bool Is.*Pressed\|Vector2 Get.*Vector\|float Get.*Delta" Assets/Scripts/Core/IInputSource.cs 2>/dev/null || echo "0")
    implemented_methods=$(grep -c "public bool\|public Vector2\|public float" "$file" 2>/dev/null || echo "0") 
    
    echo "  Interface methods: $interface_methods, Implemented: $implemented_methods"
done

echo -e "\n=== Checking for duplicate method definitions ==="
for file in Assets/Scripts/Controllers/*.cs; do
    if [ -f "$file" ]; then
        # Check for duplicate Update methods
        update_count=$(grep -c "void Update(" "$file" 2>/dev/null || echo "0")
        if [ "$update_count" -gt 1 ]; then
            echo "WARNING: $file has $update_count Update() methods"
            grep -n "void Update(" "$file"
        fi
    fi
done

echo -e "\n=== Basic syntax check complete ==="
