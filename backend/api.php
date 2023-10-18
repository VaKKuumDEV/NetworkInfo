<?php
ini_set('error_reporting', E_ALL);
ini_set('display_errors', 1);
ini_set('display_startup_errors', 1);
ini_set('max_execution_time', '300');

const EXECUTE_PATH = __DIR__;

$answer = null;
if(isset($_REQUEST['method'])){
	$method = $_REQUEST['method'];
	
	$it = new RecursiveDirectoryIterator("models");
	foreach(new RecursiveIteratorIterator($it) as $file){
		if($file->getExtension() == 'php') include $file->getPathname();
	}
	
	try{
		if($method == 'send'){
			if(isset($_REQUEST['operator'], $_REQUEST['device'], $_REQUEST['lat'], $_REQUEST['long'], $_REQUEST['speed'], $_REQUEST['strength'])){
				$operator = $_REQUEST['operator'];
				$device = $_REQUEST['device'];
				$lat = floatval($_REQUEST['lat']);
				$long = floatval($_REQUEST['long']);
				$speed = floatval($_REQUEST['speed']);
				$strength = intval($_REQUEST['strength']);
				
				RawsModel::newInstance()->sendRaw($operator, $lat, $long, $speed, $strength, $device);
				
				$answer = ['code' => 1, 'message' => 'Данные отправлены'];
			}
		}else if($method == 'map'){
			if(isset($_REQUEST['operator'], $_REQUEST['lat'], $_REQUEST['long'], $_REQUEST['radius'])){
				$operator = $_REQUEST['operator'];
				$lat = floatval($_REQUEST['lat']);
				$long = floatval($_REQUEST['long']);
				$radius = intval($_REQUEST['radius']);
				
				$points = RawsModel::newInstance()->getRaws($operator, $lat, $long, $radius);
				
				$answer = ['code' => 1, 'message' => 'Данные о точках', 'points' => $points];
			}
		}
	}catch(\Exception $ex){
		$answer = ['code' => 0, 'message' => $ex->getMessage(), 'trace' => $ex->getTrace()];
	}
}

header('Content-Type: application/json; charset=utf-8');
if($answer == null) $answer = ['code' => 0, 'message' => 'Переданы не все параметры.'];
echo json_encode($answer, JSON_UNESCAPED_UNICODE | JSON_PRETTY_PRINT);
?>