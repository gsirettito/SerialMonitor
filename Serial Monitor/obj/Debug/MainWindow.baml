   M S B A M L   `   `   `  ����H  DSerialMonitor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:SerialMonitorSerialMonitor  5   	   O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem 5   	       SerialMonitor.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:SerialMonitor     5   	   6sys&clr-namespace:System;assembly=mscorlib     5   	     � Title$  Serial Monitor��5	   	   $	��300��6    $	��625��6-    � Icon$ smallIcon.ico��69    � 	Resources 5
          BooleanToVisibilityConverter��%   &	        �� 5   
   6'   5      .��5      ]� .+�5   
   9�  9� ExpandDirection$
 Left=�6    9� 
IsExpanded . ��5      �� 5      $��0 10 0 0q�6    J� RenderTransformOrigin$ 0.5,0.5&�6*   z�5      V� 5      .~�5      �� �� 5      �� 5       �� Angle$ 90Q�6.   T� 5      5      .��5   -   Config5      .��5      �� $	��150��6   $��5 5 0 5q�6%   .+�5      �� �� 6   $�COM Port��6!   64   �� 5      -   5         ��Name$ comports��5       � ToolTip 6V   �� *SelectedValue.Description+	 �� RelativeSource	 �� *Self+��5      �� 5      .R�5       �� �� 5!   "   �6&   �� *Description+ (�� 6G   �6K   �� *Name+)5"      5#      5%      �� 5&      $��0,5,0,0q�6   �� 6.   $�
Baud rates��62   6G   �� 5'      -   5        $ combaud��5'      
 �� SelectedIndex$
 6��6,   9� 5(      $��0 5 0 0q�6   $��Advanced��6,   .��5)      �� .+�5*      �� $��0,5,0,0q�6$   �� 66   $�	Data bits��6:   6N   �� 5+      -   5        $ databits��5+   #   $
 1��65   �� 5,      $��0,5,0,0q�6$   �� 66   $�Parity��6:   6K   �� 5-      -   5        $ parity��5-   #   $
 0��63   �� 5.      $��0,5,0,0q�6$   �� 66   $�	Stop bits��6:   6N   �� 5/      -   5        $ stopbits��5/   #   $
 1��65   50      51      d� 52      -   5         d�Name$ 
btnConnect��52      $��0,7,0,0q�63   .��6�   	Connect6�   53      54      ]� 55   
   $	��Top=�6   $��0 5 5 5q�6)   .+�56      �� -   5         ��Name$
 send��56      $��Right=�6#   $	��8,3q�6:   $��5 0 0 0q�6H   .��6l   Send6r   �� 57      -   5         ��Name$ sendBox��57       �� 
CaretBrush 58      �� *
Foreground+ �� ElementName$ console����59      �� *
Foreground+$ console����5:      �� *
Background+$ console����5;      �� *BorderBrush+$ console����5<      �� *
FontFamily+$ console��5=      5>      �� 5?   
   $��Bottom=�6   .@�5@      �� -	   5         ��Name$ 
autoscroll��5@       d� 	IsChecked$
 TrueY�6+   .��6<   
Autoscroll6H   �� 5A      -
   5        $
 endl��5A      $
 1��6%   �� 5B      -   5        $
��10,2q�5B      .��68   Clear6?   �� 5C      y�6   �� *	IsChecked+ �� 	Converter# [  $ 
btnConnect���� 5D      .�6   Connected to6%   5E      �� 5F      �� 5G      �6   �� *SelectedValue.Description+$ comports�� (�� 6[   �6_   �� *SelectedValue.Name+$ comports��)5H      �� 5I      -   5         ��Name$ timer��5I      5J      �� 5K   
   -   5        $ console��5K      |�. $��0 0 5 0q�5L      $ �� HorizontalScrollBarVisibility$
 Auto=�5M      " �� VerticalScrollBarVisibility$
 Auto=�5N       �� 
IsReadOnly . 65   5O      5P      