# PdfiumViewer

Apache 2.0 License.

> Note: this is a .Net Core WPF port of [PdfiumViewer](https://github.com/bezzad/PdfiumViewer)

> I just change TargetFrameworks to .Net8 and update some of pagckages

[Download from NuGet](http://nuget.org/packages/PdfiumViewer.WPF).

![PdfiumViewer.WPF](https://raw.githubusercontent.com/vrjure/PdfiumViewer/master/screenshot.png)

![PdfiumViewer.WPF](https://raw.githubusercontent.com/vrjure/PdfiumViewer/master/screenshot2.png)

![PdfiumViewer.WPF](https://raw.githubusercontent.com/bezvrjurezad/PdfiumViewer/master/screenshot3.png)

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

* The `PdfRenderer` control. This control implements the raw PDF renderer.
  This control displays a PDF document, provides zooming and scrolling
  functionality and exposes methods to perform more advanced actions;
* The `PdfDocument` class provides access to the PDF document and wraps
  the Pdfium library.

## License

PdfiumViewer is licensed under the Apache 2.0 license. See the license details for how PDFium is licensed.