# PdfiumViewer

Apache 2.0 License.

> Note: this is a .Net Core WPF port of [PdfiumViewer](https://github.com/bezzad/PdfiumViewer)

> Here are some of the changes

- For V1.0: I just change TargetFrameworks to .Net8 and update some of pagckages
- For V2.0：Pdfrender is no longer available,it has been replaced by PDFViewer. In order to use Mvvm I add DependencyProperty.But now it's just the view function
- For V3.0: `PdfiumViewer.Native.x86.v8-xfa` and `PdfiumViewer.Native.x86_64.v8-xfa` are no longer used. I switch to [pdfium-binaries](https://github.com/bblanchon/pdfium-binaries). It's not fully functional. These are the features that have been implemented:

- [x] view
- [x] save as image
- [x] zoom
- [x] search text
- [x] bookmarks
- [x] select text
- [x] page mode switch
- [x] rotate
- [x] get page text

[Download from NuGet](https://www.nuget.org/packages/PdfiumViewer.Net.WPF).

![PdfiumViewer.WPF](https://raw.githubusercontent.com/vrjure/PdfiumViewer/master/screenshot.png)

![PdfiumViewer.WPF](https://raw.githubusercontent.com/vrjure/PdfiumViewer/master/screenshot2.png)

![PdfiumViewer.WPF](https://raw.githubusercontent.com/vrjure/PdfiumViewer/master/screenshot3.png)

## Introduction

PdfiumViewer is a PDF viewer based on the PDFium project.

PdfiumViewer provides a number of components to work with PDF files:

* PdfDocument is the base class used to render PDF documents;

* PdfRenderer is a WPF control that can render a PdfDocument;

## Compatibility

The PdfiumViewer library has been tested with Windows XP and Windows 8, and
is fully compatible with both. However, the native PDFium libraries with V8
support do not support Windows XP. See below for instructions on how to
reference the native libraries.

## Note on the `PdfViewer` control

The PdfiumViewer library primarily consists out of three components:

* The `PDFViewer` control. This control implements the raw PDF renderer.
  This control displays a PDF document, provides zooming and scrolling
  functionality and exposes methods to perform more advanced actions;
* The `PdfDocument` class provides access to the PDF document and wraps
  the Pdfium library.

## License

PdfiumViewer is licensed under the Apache 2.0 license. See the license details for how PDFium is licensed.