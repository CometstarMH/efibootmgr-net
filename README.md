# efibootmgr-net

efibootmgr, implemented/ported in C#

## Why?

The only (that I know of) Windows tool to edit EFI NVRAM is bcdedit. It does 
not provide all the options provided by efibootmgr, and mainly focuses on 
editing Windows Boot Configuration Data (BCD). This is an attempt to clone 
most of the functionalities provided by efibootmgr in Linux.

## Why C#?

Because.

I just want some practice on C#.

## Current status

Proof of concept. It can print out the default displays of efibootmgr without
any options.

## Known Limitations
Windows has no documented way to itetrate all EFI NVRAM variables. Linux or 
other Unix-like systems have `efi_get_next_variable_name` provided in efivar.
Currently, efibootmgr-net has to rely on the undocunmented`NtEnumerateBootEntries`
to get the list of existing `Boot####` variables. 