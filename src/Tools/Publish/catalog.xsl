<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns="http://www.w3.org/1999/xhtml"
		xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
		xmlns:interface="http://zero-install.sourceforge.net/2004/injector/interface"
		xmlns:catalog="http://0install.de/schema/injector/catalog"
		version="1.0">

	<xsl:output method="xml" encoding="utf-8"
		    doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"
		    doctype-public="-//W3C//DTD XHTML 1.0 Strict//EN"/>

	<xsl:template match="/catalog:catalog">
<html>

<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<meta http-equiv="Content-Language" content="en" />
<title>Software in this repository</title>
<link rel="stylesheet" href="catalog.css" type="text/css" />
</head>

<body>
<div class='main'>
<h1>Software in this repository</h1>

<p>To start one of these programs, drag the program's title from your web-browser to <a href="http://0install.net/">0install</a>.</p>

<table>
<xsl:for-each select='interface:interface'>
<tr class='d{position() mod 2}'>
<xsl:variable name='icon' select='interface:icon/@href'/>
<td class="image" width="48"><xsl:if test='$icon'><img src='{$icon}' alt="Application icon" width="48" height="48"/></xsl:if><xsl:if test='not($icon)'><img src='http://0install.net/tango/applications-system.png' alt="" width="48" height="48"/></xsl:if></td>
<td class="keyinfo">
<a href="{@uri}"><xsl:value-of select='interface:name'/></a><br/>
<span class="summary"><xsl:if test='interface:summary[@xml:lang="en"]'><xsl:value-of select='interface:summary[@xml:lang="en"]'/></xsl:if><xsl:if test='not(interface:summary[@xml:lang="en"])'><xsl:value-of select='interface:summary'/></xsl:if></span>
</td>
<td class="teaser"><xsl:if test='interface:description[@xml:lang="en"]'><xsl:value-of select='interface:description[@xml:lang="en"]'/></xsl:if><xsl:if test='not(interface:description[@xml:lang="en"])'><xsl:value-of select='interface:description'/></xsl:if>
<xsl:if test='string(interface:homepage)'><a href='{interface:homepage}'>Homepage</a></xsl:if>
</td>
</tr>
</xsl:for-each>
</table>

<p>Number of feeds: <xsl:value-of select="count(//interface:interface)"/></p>

<p>If you want to make additional programs available this way, see the
<a href="http://0install.net/packagers.html">Packaging Guide</a>.</p>

</div>
</body>

</html>
	</xsl:template>

</xsl:stylesheet>
