# CopyDir

## 概要

ディレクトリ構造を保ったままファイルをコピーします。

## 使い方

何も引数を与えずに実行するとヘルプが見れます。
```
C:\Projects\CopyDir\CopyDir
usage: CopyDir.exe destinationDir [files ...]
The source directory is current dir. You can use stdin for files. Specified files by relative path from current dir. 
Options:
  -d dry run.
  -r use regex for files.
```

コピー元は常にカレントディレクトリになります。コピー先ディレクトリを第一引数に指定します。コピーするファイルを限定したい場合は２番目以降の引数に指定します。この時、filesはコピー元ディレクトリからの相対パスで指定してください。filesのディレクトリ区切り文字は`\`でも`/`でも可能です（`/`は`¥`に置換されます）。

`-d`で実際に実行せずリストの表示のみが可能です。`-r`で引数のfilesが正規表現として評価されマッチングが行われます。

比較（正規表現のマッチングも含む）はコピー元ディレクトリを含まない、相対パスの部分で行われます。

パイプで別のコマンドからファイルリストを渡すことが可能です。

```
git ls-files -m | C:\Projects\CopyDir\CopyDir c:\path\to\destination\dir
```

パイプでのファイル指定と、引数を組み合わせることも可能です。

```
git ls-files -m | C:\Projects\CopyDir\CopyDir c:\path\to\destination\dir path/to/additional/file.txt
```

正規表現オプションは、パイプからのリダイレクト部分は無視され完全一致で比較されます。引数のみ正規表現で検索されます。

```
git ls-files -m | C:\Projects\CopyDir\CopyDir c:\path\to\destination\dir -r path/[.*]/file\.text
```

正規表現でパス区切り文字に`¥`を使う場合、2個重ねる必要があるので注意してください。

```
git ls-files -m | C:\Projects\CopyDir\CopyDir c:\path\to\destination\dir -r path\\[.*]\\file\.text
```
