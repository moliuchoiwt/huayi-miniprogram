/**
 * 常用方法封装 请求，文件上传等
 * @author echo. 
 **/
const tui = {
	//接口地址
	itfUrl: 'https://www.szcxnl.cn/api/',
	// 图片地址
	itfImgUrl: '',
	//固定token
	itfTokenUrl: '',
	baseImgUrl: function (url = "") {
		return url.includes("http") ? url : tui.itfImgUrl + url;
	},
	toast: function (text, success, duration) {
		uni.showToast({
			title: text || "出错啦~", 
			icon: success ? 'success' : 'none',
			duration: duration || 1000
		})
	},
	modal: function (title, content, callback, showCancel, cancelText, confirmText) {
		uni.showModal({
			title: content && title,
			content: content || title,
			showCancel: showCancel,
			cancelColor: "#555555",
			confirmColor: "#50aa50",
			cancelText: cancelText || "取消",
			confirmText: confirmText || "确定",
			success(res) {
				if (res.confirm) {
					callback && callback(true)
				} else {
					callback && callback(false)
				}
			}
		})
	},
	isAndroid: function () {
		const res = uni.getSystemInfoSync();
		return res.platform.toLocaleLowerCase() == "android"
	},
	isPhoneX: function () {
		const res = uni.getSystemInfoSync();
		let iphonex = false;
		let models = ['iphonex', 'iphonexr', 'iphonexsmax', 'iphone11', 'iphone11pro', 'iphone11promax']
		const model = res.model.replace(/\s/g, "").toLowerCase()
		if (models.includes(model)) {
			iphonex = true;
		}
		return iphonex;
	},
	constNum: function () {
		let time = 0;
		// #ifdef APP-PLUS
		time = this.isAndroid() ? 300 : 0;
		// #endif
		return time
	},
	showLoading: function (title, mask = true) {
		uni.showLoading({
			mask: mask,
			title: title || '请稍候...'
		})
	},
	/**
	 * 请求数据处理
	 * @param string url 请求地址
	 * @param {*} postData 请求参数
	 * @param {*} headers 增加请求头参数
	 * @param bool isForm 数据格式
	 *  true: 'application/x-www-form-urlencoded'
	 *  false:'application/json' 
	 * @param bool hideLoading 是否显示loading
	 *  true: 隐藏
	 *  false:显示
	 */
	request: async function (url, postData, headers, isForm, hideLoading) {
		//接口请求
		let loadding = false;
		if (hideLoading) {
			loadding = true
			uni.showLoading({
				mask: true,
				title: '请稍候...'
			})
		}
		return new Promise((resolve, reject) => {
			uni.request({
				url: tui.itfUrl + url,
				data: postData || {},
				header: {
					'content-type': isForm ? 'application/x-www-form-urlencoded' :
						'application/json',
					'Authorization': "Bearer   " + tui.getToken(),
					"version": '1.0.0',
					...headers
				},
				method: "POST",
				dataType: 'json',
				success: (res) => {
					let rs = res.data
					if (rs.code === 50008 || rs.code == 501) {
						uni.clearStorageSync()
						tui.modal('登录', '进行登录操作', callback => {
							if (callback) {
								uni.redirectTo({
									url: '/pages/login/index'
								})
							} else {
								tui.back()
							}
						}, true, '暂不登录', '前往登录')
						return
					}
					if (rs.code != 200) {
						tui.toast(rs.msg)
						reject(rs)
					} else {
						resolve(rs)
					}
				},
				fail: (res) => {
					tui.toast("网络不给力，请稍后再试~")
					reject(res)
				},
				complete: () => {
					if (hideLoading) {
						uni.hideLoading()
					}
				}
			})
		})
	},
	/**
	 * 上传文件
	 * @param string src 文件路径
	 */
	uploadFile: function (src) {
		return new Promise((resolve, reject) => {
			const uploadTask = uni.uploadFile({
				url: tui.itfUrl + 'Upload/APiUploadFile',
				filePath: src,
				name: 'file',
				header: {
					'Authorization': "Bearer   " + tui.getToken()
				},
				formData: {
					// sizeArrayText:""
				},
				success: function (res) {
					let d = JSON.parse(res.data.replace(/\ufeff/g, "") || "{}")
					if (d.code == 200) {
						//返回图片地址
						let fileObj = d.data;
						resolve(fileObj)
					} else {
						tui.toast(d.msg);
					}
				},
				fail: function (res) {
					reject(res)
					tui.toast(res.errMsg);
				}
			})
		})
	},
	setToken(token) {
		uni.setStorageSync("http_token", token)
	},
	//获取token
	getToken() {
		return uni.getStorageSync("http_token")
	},
	//判断是否登录
	isLogin: function () {
		return uni.getStorageSync("http_token") ? true : false
	},
	//跳转页面，校验登录状态
	href(url, isVerify) {
		if (isVerify && !tui.isLogin()) {
			tui.modal('登录', '进行登录操作', callback => {
				if (callback) {
					uni.navigateTo({
						url: '/pages/login/index'
					})
				}
			}, true, '暂不登录', '前往登录')
		} else {
			uni.navigateTo({
				url
			});
		}
	},
	// 获取文件大小 size bypes
	sizeMB(size) {
		if (size < 1024) {
			return size + 'B';
		} else if (size / 1024 >= 1 && size / 1024 / 1024 < 1) {
			return Math.floor(size / 1024 * 100) / 100 + 'KB';
		} else if (size / 1024 / 1024 >= 1) {
			return Math.floor(size / 1024 / 1024 * 100) / 100 + 'MB';
		}
	},
	// 时间戳转换年月
	timeYearMonth(value) {
		var date = new Date(value);
		let queryYear = date.getFullYear()
		let queryMonth = date.getMonth() + 1
		return [queryYear, queryMonth]
	},
	/**
	 * 支付
	 * @param {Object} data 数据
	 * @param {Object} link 跳转链接
	 */
	payment(res, link) {
		if (res.code == 200) {
			if (res.data.IsPay) {
				var wxData = res.data.wxData
				uni.requestPayment({
					provider: 'wxpay',
					timeStamp: wxData.timeStamp,
					nonceStr: wxData.nonceStr,
					package: wxData.package,
					signType: wxData.signType,
					paySign: wxData.paySign,
					success: (resRequest) => {
						tui.toast('支付成功')
						if (link) {
							setTimeout(() => {
								if (link.includes('/pages/order/index')) {
									uni.reLaunch({
										url: link
									})
									return
								}
								uni.redirectTo({
									url: link
								})
							}, 400)
						}
					},
					fail: (err) => {
						tui.toast('支付失败')
						console.log(err)
					},
					complete: () => { }
				});
			} else {
				//无需支付
				uni.redirectTo({
					url: link
				})
			}
		}
	},
	/**
	 * 重新授权并调用定位方法
	 * @param {Function} sucFun 授权成功回调
	 * @param {string} strName 授权权限
	 */
	getAuthorize(sucFun, strName) {
		let name = 'scope.userLocation'
		if (strName) {
			name = strName
		}
		uni.authorize({
			scope: name,
			success: () => {
				sucFun()
			},
			fail: (err) => {
				err = err['errMsg']
				tui.modal('定位授权', '需要授权位置信息', callback => {
					if (callback) {
						uni.openSetting({
							success: (res) => {
								if (res.authSetting[name]) {
									sucFun()
								} else {
									tui.toast('授权失败')
								}
							}
						})
					} else {
						tui.toast('授权失败')
					}
				})
			}
		})
	},
	back() {
		let pages = getCurrentPages(); // 获取当前页面栈的实例
		let currPage = pages[pages.length - 1]; //当前页面
		let prevPage = pages[pages.length - 2]; //上一个页面

		//判断上一页是否为首页，如果是就直接返回首页
		if (prevPage && prevPage.route && prevPage.route.indexOf("/jump") != -1) {
			uni.switchTab({
				url: '/pages/index/index' //路径为测试数据，填写小程序真实路径就行
			});
			return;
		}

		//uni.navigateBack() 关闭当前页面，返回上一页面或多级页面。可通过 getCurrentPages() 获取当前的页面栈，决定需要返回几层
		uni.navigateBack({
			success: () => {
				delta: 1 //返回的页面数，如果 delta 大于现有页面数，则返回到首页。默认为1
				//console.log('success')
			},

			//失败回调直接返回首页
			fail: () => {
				//console.log('fail')
				uni.switchTab({
					url: '/pages/index/index' //路径为测试数据，填写小程序真实路径就行
				})
			}
		})
	},
	/**
	   * 格式化位置
	   * @param {*} res chooseLocation成功后返回参数
	   * 格式: {
			 address: "山东省济南市槐荫区经十西路29851号"
		 errMsg: "chooseLocation:ok"
		 latitude: 36.65142
		 longitude: 116.90084
		 name: "济南市槐荫区人民政府"
		 }
	   */
	captureLocation(res) {
		// console.log('res', res);
		var regex = /^(北京市|天津市|重庆市|上海市|香港特别行政区|澳门特别行政区)/;
		var province = [];
		var addressBean = {
			province: null,
			area: null,
			city: null,
			address: null,
		};

		function regexAddressBean(address, addressBean) {
			// console.log('address', address);
			// console.log('addressBean', addressBean);
			regex = /^(.*?[市]|.*?[州]|.*?地区|.*?特别行政区)(.*?[区]|.*?[市]|.*?[县])(.*?)$/g;
			var addxress = regex.exec(address);
			addressBean.city = addxress[1];
			addressBean.area = addxress[2];
			addressBean.address = addxress[3] + '(' + res.name + ')';
			// console.log(addxress);
		}
		if (!(province = regex.exec(res.address))) {
			regex = /^(.*?(省|自治区))(.*?)$/;
			province = regex.exec(res.address);
			addressBean.province = province[1];
			regexAddressBean(province[3], addressBean);
		} else {
			addressBean.province = province[1];
			regexAddressBean(res.address, addressBean);
		}
		return addressBean;
	}

}

export default tui