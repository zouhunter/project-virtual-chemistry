
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MegaXMLValue
{
	public string	name;
	public string	value;
}

public class MegaXMLNode
{
	public String				tagName;
	public MegaXMLNode			parentNode;
	public List<MegaXMLNode>	children;
	public List<MegaXMLValue>	values;

	public MegaXMLNode()
	{
		tagName = "NONE";
		parentNode = null;
		children = new List<MegaXMLNode>();
		values = new List<MegaXMLValue>();
	}
}

public class MegaXMLReader
{
	private static char TAG_START = '<';
	private static char TAG_END = '>';
	private static char SPACE = ' ';
	private static char QUOTE = '"';
	private static char SLASH = '/';
	private static char EQUALS = '=';
	private static String BEGIN_QUOTE = "" + EQUALS + QUOTE;

	public MegaXMLReader()
	{
	}

	public MegaXMLNode read(String xml)
	{
		int index = 0;
		int lastIndex = 0;
		MegaXMLNode rootNode = new MegaXMLNode();
		MegaXMLNode currentNode = rootNode;

		xml = xml.Replace(" \n", "");
		xml = xml.Replace("\n", "");

		while ( true )
		{
			index = xml.IndexOf(TAG_START, lastIndex);

			if ( index < 0 || index >= xml.Length )
				break;

			index++;

			lastIndex = xml.IndexOf(TAG_END, index);
			if ( lastIndex < 0 || lastIndex >= xml.Length )
				break;

			int tagLength = lastIndex - index;
			String xmlTag = xml.Substring(index, tagLength);

			if ( xmlTag[0] == SLASH )
			{
				currentNode = currentNode.parentNode;
				continue;
			}

			bool openTag = true;

			if ( xmlTag[tagLength - 1] == SLASH )
			{
				xmlTag = xmlTag.Substring(0, tagLength - 1);
				openTag = false;
			}


			MegaXMLNode node = parseTag(xmlTag);
			node.parentNode = currentNode;
			currentNode.children.Add(node);

			if ( openTag )
				currentNode = node;
		}

		return rootNode;
	}


	public MegaXMLNode parseTag(String xmlTag)
	{
		MegaXMLNode node = new MegaXMLNode();

		int nameEnd = xmlTag.IndexOf(SPACE, 0);
		if ( nameEnd < 0 )
		{
			node.tagName = xmlTag;
			return node;
		}

		String tagName = xmlTag.Substring(0, nameEnd);
		node.tagName = tagName;

		String attrString = xmlTag.Substring(nameEnd, xmlTag.Length - nameEnd);
		return parseAttributes(attrString, node);
	}

	public MegaXMLNode parseAttributes(String xmlTag, MegaXMLNode node)
	{
		int index = 0;
		int attrNameIndex = 0;
		int lastIndex = 0;

		while ( true )
		{
			index = xmlTag.IndexOf(BEGIN_QUOTE, lastIndex);
			if ( index < 0 || index > xmlTag.Length )
				break;

			attrNameIndex = xmlTag.LastIndexOf(SPACE, index);
			if ( attrNameIndex < 0 || attrNameIndex > xmlTag.Length )
				break;

			attrNameIndex++;
			String attrName = xmlTag.Substring(attrNameIndex, index - attrNameIndex);

			index += 2;

			lastIndex = xmlTag.IndexOf(QUOTE, index);
			if ( lastIndex < 0 || lastIndex > xmlTag.Length )
			{
				break;
			}

			int tagLength = lastIndex - index;
			String attrValue = xmlTag.Substring(index, tagLength);

			MegaXMLValue val = new MegaXMLValue();
			val.name = attrName;
			val.value = attrValue;
			node.values.Add(val);
		}

		return node;
	}
}
