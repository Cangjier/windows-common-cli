# Windows Common CLI

# 脚本

## run

```
wcl.exe run {ScriptPath}
```

## package

```
wcl.exe package {InputDirectory}
```

## bat

```
wcl.exe bat {ScriptPath}
```

# 压缩/解压缩

## extract

```bash
wcl.exe extract {CompressFilePath} {OutputDirectory}
```

- CompressFilePath: 压缩包文件路径
- OutputDirectory: 输出目录

## compress

```
wcl.exe compress {CompressFilePath} {Paths}
```

- CompressFilePath: 压缩包文件路径
- Paths:待压缩的文件路径，支持多个，如`path1 path2`

# Windows窗口操作

- hWnd
  - 整数，窗口句柄
  - 0xFFFFFFFF，十六进制表达的窗口句柄
  - g/Button，通过正则匹配WindowText/Text/ClassName

## list-windows

```
wcl.exe list-windows {OutputPath}
```

## list-user-windows

```
wcl.exe list-user-windows {OutputPath}
```

## list-children-windows

```
wcl.exe list-children-windows {hWnd} {OutputPath}
```

## click-window

```
wcl.exe click-window {hWnd} --method {Method} --count {Count}
```

- Method：点击的方案，0-3，默认是0
- Count：点击的次数，默认是1

## set-window-text

```
wcl.exe set-window-text {hWnd} {Text} --type {Type}
```

- Type 类型，text有不同的类型
  - window-text：一般是窗口标题
  - text：一般是文本框内容

## find-window-text/find-window-title

```
wcl.exe find-window-text {Regex} {OutputPath}
wcl.exe find-window-title {Regex} {OutputPath}
```

## find-child-window

```
wcl.exe find-child-window {hWnd} {Regex} {OutputPath}
```

## find-child-window-combobox

```
wcl.exe find-child-window-combobox {hWnd} {OutputPath}
```

## click-child-window-text

```
wcl.exe click-child-window-text {hWnd} {Regex} --method {Method} --count {Count}
```

- Method：点击的方案，0-3，默认是0
- Count：点击的次数，默认是1

## set-child-window-text

根据正则去匹配子窗口的WindowText/Text/ClassName，并设置文本

```
wcl.exe set-child-window-text-class-name {hWnd} {RegexString} {Text} --type {window-text/text}
```

## send-child-window-keys

```
wcl.exe send-child-window-keys {hWnd} {RegexString} {Keys}
```

## send-text

```
wcl.exe send-text {Text}
```

## send-keys

```
wcl.exe send-keys {hWnd} {Keys}
```

## active-english-keyboard

激活英文输入法

```
wcl.exe active-english-keyboard
```

## match-window

匹配Window状态

```
wcl.exe match-window {InputPath} {OutputPath}
```

- InputPath : WindowMatchState

```ts
export interface Window{
	Text?:string,
    Title?:string,
    ClassName?:string,
    Enable?:"true"|"false",
    Interval?:number,//匹配时间间隔
    Timeout?:number,//匹配超时设定
}

export interface WindowMatchState {
    [key:string]:Window[] | Window
}
```

## close-window

```
wcl.exe close-window {hWnd}
```

## ocr-window

```
wcl.exe ocr-window {hWnd} {OutputPath}
```

## mouse-move

```
wcl.exe mouse-move {DeltaX} {DeltaY}
```

## mouse-move-to

```
wcl.exe mouse-move-to {X} {Y}
```

## mouse-click

```
wcl.exe mouse-click
```

# Markdown

## markdown-increase

```
wcl.exe markdown-increase
```

将会对剪切板内容的Markdown中所有标题增加一级。

# 进程

## kill-process

```
wcl.exe kill-process {RegexString}
```

