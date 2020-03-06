<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Settings.ascx.cs" Inherits="FortyFingers.DnnMassManipulate.Settings" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<table width="550" cellspacing="0" cellpadding="4" border="0" width=100%>
	<tr>
		<td class="SubHead" width="150" valign="top"><dnn:label id="plSetting1" controlname="txtSetting1" runat="server" Text="setting1" suffix=":" /></td>
		<td valign="top">
		    <asp:TextBox ID="txtSetting1" runat="server" CssClass="NormalTextBox" Width="300"></asp:TextBox>
		</td>
	</tr>
</table>

