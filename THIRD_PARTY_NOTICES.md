# Source baselines

The Web Forms solution identity, .NET Framework target and assembly metadata are
derived from [SimonGelbart/playright-test](https://github.com/SimonGelbart/playright-test)
at `c82e5f0fbc4603fc0c67e09060d87b518722200f`. Its original application is the
Microsoft `dotnet-framework-docker` ASP.NET sample at
`b473d3e477ddee69fabfcf70f09f7a6c9c6934cf`. The upstream MIT notice is retained below.
The sample's unrelated pages, JS libraries, Aspire orchestration and original
homepage tests are omitted so the fixture needs no NuGet restore to build.

The LDAP setup follows the OpenLDAP 2.6.10 container approach used by
`SimonGelbart/ldap-testcontainer` at `7b1518fd1a3101f2f7ef7bb6834378c00a142258`.
The container recipe and synthetic schema/data here are newly written; no private
schema, captures, credentials, Python converter or testkit source is copied.
The base image is [vegardit/openldap](https://github.com/vegardit/docker-openldap).

## Microsoft sample notice

+The MIT License (MIT)

Copyright (c) 2016 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
