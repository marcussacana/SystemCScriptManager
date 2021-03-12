# Help-me keep actively creating tools!
### Read more about this in my Ko-fi goal:
<a href='https://ko-fi.com/Z8Z231I4Z' target='_blank'><img height='40' style='border:0px;height:40px;' src='https://cdn.ko-fi.com/cdn/kofi1.png?v=2' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a>

# SystemCScriptManager
A tool created to allow add and remove lines from SystemC Engine Script's,  
Tested with **Tsuushinbo \~Mama ni mo Naisho no Jikanwari\~**

### Usage:
- To **Extract**  
 Drag&Drop the .txt script **inside of the script dir** to the **SystemCScriptTool.exe**
- To **Insert**  
 Drag&Drop the _dump.txt file **inside of the script dir** to the **SystemCScriptTool.exe**

### Note:
The tool only work if the dragged script is in the same directory of all others scripts!  
You can't move the script from the extracted directory to insert too.

### Note 2:
Don't re-insert the text in a already modified script!  
If you need update I recommend you extract the original script again and replace the modified.

### Note 3:
Due the fact of the script is splited in many binaries, this tool isn't compatible with my [SacanaWrapper](https://github.com/marcussacana/SacanaWrapper)  
But you can install the plugin "Plain Text" and use the SacanaWrapper with the dump.

### Syntaxs:
You can't add or remove lines in the exported "_Dump.txt" file, to this you use those syntaxes.
#### New Line Example:
To add new lines put a '\n' anywhere
- Original Lines
```
『校長室』とプレートがかけられた教室の前で、
僕は何度か呼吸を落ち着かせて、
いざ、ドアを叩く。
```
- Modified Lines
```
『校長室』とプレートがかけられた教室の前で、
僕は何度か呼吸\nを落ち着かせて、
いざ、ドアを叩く。
```
- In Game Result
```
『校長室』とプレートがかけられた教室の前で、
僕は何度か呼吸
を落ち着かせて、
いざ、ドアを叩く。
```

#### Line Remove Example:
To remove a line, put <-- in the begin of the line
```
『校長室』とプレートがかけられた教室の前で、
僕は何度か呼吸を落ち着かせて、
いざ、ドアを叩く。
```
- Modified Lines
```
『校長室』とプレートがかけられた教室の前で、
僕は何度か呼吸を落ち着かせて、
<--いざ、ドアを叩く。
```
- In Game Result
```
『校長室』とプレートがかけられた教室の前で、
僕は何度か呼吸を落ち着かせて、いざ、ドアを叩く。
```
> Note: When you remove a line, a space **isn't** added between the 2 lines,  
If you want add a space between the lines, modify the [code here](https://github.com/marcussacana/SystemCScriptManager/blob/master/SystemCScriptManager/ParserWrapper.cs#L43)
