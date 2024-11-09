# 1. Hello world

## 1.1 构建环境

让我们构建`Hello World`程序，首先创建一个文件夹，将准备好的ts文件拷贝进去。

- System
- TidyHPC
- context.ts
- reflection.ts

![image-20240903174901246](static\image-20240903174901246.png)

## 1.2 main.ts

在同目录下，新建`main.ts`，并编写以下代码：

```
let main=()=>{
	console.log(`Hello world`);
};
main();
```

## 1.3 运行

```
wcl.exe run HelloWorld/main.ts
```

# 2. 使用C#标准库

```
import { Console } from "../System/Console";

let line = Console.ReadLine();
Console.WriteLine(`the line is ${line}`);
```



